using Microsoft.Win32;
using SIQuester.ViewModel;
using SIQuester.ViewModel.Contracts;
using SIQuester.ViewModel.PlatformSpecific;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace SIQuester.Behaviors;

/// <summary>
/// Right-click menu "Save as…" on a picture, sound, video or the pack logo: saves the file from the pack
/// to a folder the user picks. The media comes from the element's DataContext
/// (<see cref="ContentItemViewModel" /> or <see cref="PackageViewModel" /> for the logo).
/// Usage: <c>lb:MediaExport.IsEnabled="True"</c>.
/// </summary>
public static class MediaExport
{
    public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);

    public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);

    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
        "IsEnabled", typeof(bool), typeof(MediaExport), new PropertyMetadata(false, OnIsEnabledChanged));

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element || e.NewValue is not true)
        {
            return;
        }

        var save = new MenuItem
        {
            Header = UiText.MediaSaveAs,
            Icon = new TextBlock { Text = "", FontFamily = new System.Windows.Media.FontFamily("Segoe Fluent Icons, Segoe MDL2 Assets") },
        };

        save.Click += (_, _) => Save(element.DataContext);

        var menu = new ContextMenu();
        menu.Items.Add(save);

        // Only when there is a file to save (not for an external link or an empty logo)
        menu.Opened += (_, _) => save.IsEnabled = TryGetMedia(element.DataContext, out _, out _);

        element.ContextMenu = menu;
    }

    private static bool TryGetMedia(object? context, out IMedia? media, out string fileName)
    {
        media = null;
        fileName = "";

        switch (context)
        {
            case ContentItemViewModel item when item.Model.IsRef:
                media = item.MediaSource;
                fileName = item.Model.Value;
                break;

            case PackageViewModel package when package.Logo != null:
                media = package.Logo;
                fileName = package.Model.Logo?.TrimStart('@') ?? "";
                break;
        }

        return media != null && (media.GetStream != null || File.Exists(media.Uri));
    }

    private static void Save(object? context)
    {
        if (!TryGetMedia(context, out var media, out var fileName) || media == null)
        {
            return;
        }

        var extension = Path.GetExtension(fileName);

        var dialog = new SaveFileDialog
        {
            Title = UiText.MediaSaveAs,
            FileName = Path.GetFileName(fileName),
            Filter = string.IsNullOrEmpty(extension)
                ? UiText.MediaAllFiles + "|*.*"
                : $"{extension.TrimStart('.').ToUpperInvariant()} (*{extension})|*{extension}|{UiText.MediaAllFiles}|*.*",
        };

        if (dialog.ShowDialog(Application.Current.MainWindow) != true)
        {
            return;
        }

        try
        {
            if (media.GetStream != null)
            {
                using var source = media.GetStream().Stream;
                using var target = File.Create(dialog.FileName);
                source.CopyTo(target);
            }
            else
            {
                File.Copy(media.Uri, dialog.FileName, overwrite: true);
            }
        }
        catch (Exception exc)
        {
            PlatformManager.Instance.ShowExclamationMessage($"{UiText.MediaSaveError}: {exc.Message}");
        }
    }
}
