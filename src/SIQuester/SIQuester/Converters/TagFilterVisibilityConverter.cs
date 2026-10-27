using SIQuester.ViewModel.Helpers;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SIQuester.Converters;

/// <summary>
/// Hides tags (and whole tag groups) that do not match the text typed in the "new tag" box.
/// Values: [0] a tag or a tag group, [1] the filter text.
/// </summary>
public sealed class TagFilterVisibilityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var filter = (values.Length > 1 ? values[1] as string : null)?.Trim();

        if (string.IsNullOrEmpty(filter))
        {
            return Visibility.Visible;
        }

        bool Matches(string name) => name.Contains(filter, StringComparison.CurrentCultureIgnoreCase);

        var visible = values[0] switch
        {
            TagsRepository.Tag tag => Matches(tag.Name),
            TagsRepository.TagGroup group => Matches(group.Name) || group.Tags.Any(t => Matches(t.Name)),
            _ => true
        };

        return visible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
