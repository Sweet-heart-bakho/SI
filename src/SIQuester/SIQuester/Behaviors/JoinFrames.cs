using SIQuester.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace SIQuester.Behaviors;

/// <summary>
/// Draws one rounded frame around content items that are joined to play together
/// ("join with next"), so the group is visible at a glance.
/// The frame follows the outline of the joined items only: items laid out side by side
/// that are not in the group stay outside of it.
/// Usage on the content ListBox: <c>lb:JoinFrames.IsEnabled="True"</c>.
/// </summary>
public static class JoinFrames
{
    public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);

    public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);

    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
        "IsEnabled", typeof(bool), typeof(JoinFrames), new PropertyMetadata(false, OnIsEnabledChanged));

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ItemsControl list || e.NewValue is not true)
        {
            return;
        }

        list.Loaded += (_, _) =>
        {
            var layer = AdornerLayer.GetAdornerLayer(list);

            if (layer == null || layer.GetAdorners(list)?.OfType<FramesAdorner>().Any() == true)
            {
                return;
            }

            var adorner = new FramesAdorner(list);
            layer.Add(adorner);

            // Joining/unjoining changes the link icon and therefore the layout: redraw after every layout pass
            list.LayoutUpdated += (_, _) => adorner.Refresh();
        };
    }

    private sealed class FramesAdorner : Adorner
    {
        /// <summary>
        /// How far the frame goes around each item. Also closes the small gaps between neighbouring items,
        /// so a group becomes one shape.
        /// </summary>
        private const double Inflate = 4;

        private readonly ItemsControl _list;

        /// <summary>
        /// Item bounds of every group.
        /// </summary>
        private List<List<Rect>> _groups = new();

        public FramesAdorner(ItemsControl list) : base(list)
        {
            _list = list;
            IsHitTestVisible = false;
        }

        public void Refresh()
        {
            var groups = ComputeGroups();

            if (!SameGroups(groups, _groups))
            {
                _groups = groups;
                InvalidateVisual();
            }
        }

        private static bool SameGroups(List<List<Rect>> a, List<List<Rect>> b) =>
            a.Count == b.Count && a.Zip(b).All(pair => pair.First.SequenceEqual(pair.Second));

        private List<List<Rect>> ComputeGroups()
        {
            var result = new List<List<Rect>>();
            List<Rect>? chain = null;

            for (var i = 0; i < _list.Items.Count; i++)
            {
                if (_list.Items[i] is not ContentItemViewModel item
                    || _list.ItemContainerGenerator.ContainerFromIndex(i) is not FrameworkElement container
                    || !container.IsVisible)
                {
                    chain = null;
                    continue;
                }

                var bounds = new Rect(container.TranslatePoint(new Point(0, 0), _list), container.RenderSize);
                var joinsNext = !item.Model.WaitForFinish && i < _list.Items.Count - 1;

                if (chain != null)
                {
                    chain.Add(bounds); // continue the current group with this item
                }
                else if (joinsNext)
                {
                    chain = new List<Rect> { bounds };
                }

                if (!joinsNext && chain != null)
                {
                    result.Add(chain);
                    chain = null;
                }
            }

            return result;
        }

        /// <summary>
        /// Outline of a group: union of the (slightly enlarged, rounded) item rectangles.
        /// </summary>
        private static Geometry BuildOutline(List<Rect> items)
        {
            Geometry outline = Geometry.Empty;

            foreach (var rect in items)
            {
                var r = rect;
                r.Inflate(Inflate, Inflate);
                var piece = new RectangleGeometry(r, 6, 6);

                outline = outline.IsEmpty()
                    ? piece
                    : Geometry.Combine(outline, piece, GeometryCombineMode.Union, null);
            }

            outline.Freeze();
            return outline;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (_groups.Count == 0)
            {
                return;
            }

            var accent = _list.TryFindResource("Ui.Accent") as SolidColorBrush ?? Brushes.SteelBlue;

            var fill = new SolidColorBrush(Color.FromArgb(0x14, accent.Color.R, accent.Color.G, accent.Color.B));
            fill.Freeze();

            var pen = new Pen(accent, 1.5) { DashStyle = new DashStyle(new double[] { 4, 2 }, 0), LineJoin = PenLineJoin.Round };
            pen.Freeze();

            foreach (var group in _groups)
            {
                drawingContext.DrawGeometry(fill, pen, BuildOutline(group));
            }
        }
    }
}
