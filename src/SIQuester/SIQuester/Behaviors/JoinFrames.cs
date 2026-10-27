using SIQuester.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace SIQuester.Behaviors;

/// <summary>
/// Draws one rounded frame around content items that are joined to play together
/// ("join with next"), so the group is visible at a glance.
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
        private const double Inflate = 3;

        private readonly ItemsControl _list;
        private readonly List<Rect> _frames = new();

        public FramesAdorner(ItemsControl list) : base(list)
        {
            _list = list;
            IsHitTestVisible = false;
        }

        public void Refresh()
        {
            var frames = ComputeFrames();

            if (!frames.SequenceEqual(_frames))
            {
                _frames.Clear();
                _frames.AddRange(frames);
                InvalidateVisual();
            }
        }

        private List<Rect> ComputeFrames()
        {
            var result = new List<Rect>();
            Rect? chain = null;

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

                if (chain.HasValue)
                {
                    // Continue the current group with this item
                    var union = chain.Value;
                    union.Union(bounds);
                    chain = union;
                }
                else if (joinsNext)
                {
                    chain = bounds;
                }

                if (!joinsNext && chain.HasValue)
                {
                    var frame = chain.Value;
                    frame.Inflate(Inflate, Inflate);
                    result.Add(frame);
                    chain = null;
                }
            }

            return result;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (_frames.Count == 0)
            {
                return;
            }

            var accent = _list.TryFindResource("Ui.Accent") as SolidColorBrush ?? Brushes.SteelBlue;

            var fill = new SolidColorBrush(Color.FromArgb(0x14, accent.Color.R, accent.Color.G, accent.Color.B));
            fill.Freeze();

            var pen = new Pen(accent, 1.5) { DashStyle = new DashStyle(new double[] { 4, 2 }, 0) };
            pen.Freeze();

            foreach (var frame in _frames)
            {
                drawingContext.DrawRoundedRectangle(fill, pen, frame, 6, 6);
            }
        }
    }
}
