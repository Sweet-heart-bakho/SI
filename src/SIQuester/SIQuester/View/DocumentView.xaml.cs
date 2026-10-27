using SIQuester.Model;
using System.Windows;
using System.Windows.Controls;

namespace SIQuester;

/// <summary>
/// Логика взаимодействия для DocumentView.xaml
/// </summary>
public partial class DocumentView : UserControl
{
    public DocumentView() => InitializeComponent();

    private void FontSmaller_Click(object sender, RoutedEventArgs e) => ChangeFontSize(-1);

    private void FontBigger_Click(object sender, RoutedEventArgs e) => ChangeFontSize(+1);

    private void FontReset_Click(object sender, RoutedEventArgs e) => ResetFontSize();

    // Switches light <-> dark (the same setting as in Settings → Theme)
    private void ToggleTheme_Click(object sender, RoutedEventArgs e) =>
        AppSettings.Default.Theme = AppSettings.Default.Theme == ThemeOption.Light ? ThemeOption.Dark : ThemeOption.Light;

    internal const int DefaultFontSize = 16;

    /// <summary>
    /// Restores the default text size.
    /// </summary>
    internal static void ResetFontSize() => AppSettings.Default.FontSize = DefaultFontSize;

    /// <summary>
    /// Changes the document text size (kept in settings).
    /// </summary>
    internal static void ChangeFontSize(int delta)
    {
        var settings = AppSettings.Default;
        settings.FontSize = Math.Clamp(settings.FontSize + delta, 10, 32);
    }
}
