using SIPackages;
using SIQuester.Model;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace SIQuester.Helpers;

/// <summary>
/// Makes media files fit the SIGame size limits (picture 1 MB, audio 5 MB, video 10 MB).
/// Pictures are handled with the built-in WPF codecs; audio and video need FFmpeg (ffmpeg.exe in PATH,
/// next to the program, or installed with winget).
/// </summary>
internal static class MediaShrinker
{
    /// <summary>
    /// Returns a compressed copy that fits <paramref name="maxBytes" />, or null when it can't be done.
    /// </summary>
    public static string? Shrink(string path, string collectionName, long maxBytes) => collectionName switch
    {
        CollectionNames.ImagesStorageName => ShrinkImage(path, maxBytes),
        CollectionNames.VideoStorageName => ShrinkWithFfmpeg(path, maxBytes, isVideo: true),
        CollectionNames.AudioStorageName => ShrinkWithFfmpeg(path, maxBytes, isVideo: false),
        _ => null
    };

    // ----- Pictures -----

    /// <summary>
    /// Re-encodes the picture: first lowers JPEG quality, then reduces the size step by step until it fits.
    /// Pictures with transparency stay PNG; others become JPEG. Animated GIF and AVIF are left as is.
    /// </summary>
    private static string? ShrinkImage(string path, long maxBytes)
    {
        var extension = Path.GetExtension(path).ToLowerInvariant();

        if (extension is ".gif" or ".avif")
        {
            return null; // would lose animation / no encoder
        }

        BitmapSource source;

        try
        {
            var decoder = BitmapDecoder.Create(new Uri(path), BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.OnLoad);
            source = decoder.Frames[0];
        }
        catch
        {
            return null; // format not supported by Windows codecs
        }

        var keepTransparency = HasTransparency(source);

        // Larger than a screen makes no sense in the game
        var longSide = Math.Max(source.PixelWidth, source.PixelHeight);
        var scale = Math.Min(1.0, 2560.0 / longSide);

        for (var attempt = 0; attempt < 14; attempt++)
        {
            BitmapSource frame = scale < 0.999
                ? new TransformedBitmap(source, new ScaleTransform(scale, scale))
                : source;

            byte[] data;

            if (keepTransparency)
            {
                data = Encode(new PngBitmapEncoder(), frame);
            }
            else
            {
                var opaque = new FormatConvertedBitmap(frame, PixelFormats.Bgr24, null, 0);
                data = Array.Empty<byte>();

                foreach (var quality in new[] { 88, 80, 72, 64 })
                {
                    data = Encode(new JpegBitmapEncoder { QualityLevel = quality }, opaque);

                    if (data.Length <= maxBytes)
                    {
                        break;
                    }
                }
            }

            if (data.Length <= maxBytes)
            {
                var output = CreateOutputPath(path, keepTransparency ? ".png" : ".jpg");
                File.WriteAllBytes(output, data);
                return output;
            }

            scale *= 0.8;
        }

        return null;
    }

    private static byte[] Encode(BitmapEncoder encoder, BitmapSource frame)
    {
        encoder.Frames.Add(BitmapFrame.Create(frame));
        using var stream = new MemoryStream();
        encoder.Save(stream);
        return stream.ToArray();
    }

    /// <summary>
    /// Whether the picture really uses transparency (an alpha channel alone is not enough).
    /// </summary>
    private static bool HasTransparency(BitmapSource source)
    {
        var format = source.Format;

        if (format != PixelFormats.Bgra32 && format != PixelFormats.Pbgra32 && format != PixelFormats.Prgba64
            && format != PixelFormats.Rgba64 && format != PixelFormats.Prgba128Float && format != PixelFormats.Rgba128Float
            && format != PixelFormats.Indexed1 && format != PixelFormats.Indexed2 && format != PixelFormats.Indexed4 && format != PixelFormats.Indexed8)
        {
            return false;
        }

        var bgra = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
        var stride = bgra.PixelWidth * 4;
        var pixels = new byte[stride * bgra.PixelHeight];
        bgra.CopyPixels(pixels, stride, 0);

        for (var i = 3; i < pixels.Length; i += 4)
        {
            if (pixels[i] < 250)
            {
                return true;
            }
        }

        return false;
    }

    // ----- Audio and video (FFmpeg) -----

    private static bool _ffmpegHintShown;

    /// <summary>
    /// Re-encodes with FFmpeg to a bitrate that fits the limit (video: H.264 + AAC in MP4, up to 1280 px wide;
    /// audio: MP3). The UI stays responsive while FFmpeg works.
    /// </summary>
    private static string? ShrinkWithFfmpeg(string path, long maxBytes, bool isVideo)
    {
        var ffmpeg = FindTool("ffmpeg.exe");

        if (ffmpeg == null)
        {
            if (!_ffmpegHintShown)
            {
                _ffmpegHintShown = true;
                ViewModel.PlatformSpecific.PlatformManager.Instance.Inform(ViewModel.UiText.FfmpegMissing, true);
            }

            return null;
        }

        var duration = GetDurationSeconds(path, ffmpeg);

        if (duration is null or <= 0)
        {
            return null;
        }

        // 92 % of the limit for the streams, the rest for the container
        var totalKbps = maxBytes * 8.0 / 1000.0 * 0.92 / duration.Value;
        var audioKbps = isVideo ? Math.Min(96, Math.Max(48, totalKbps * 0.15)) : Math.Min(192, totalKbps);
        var videoKbps = totalKbps - audioKbps;

        if (audioKbps < 32 || (isVideo && videoKbps < 100))
        {
            return null; // too long to fit with acceptable quality
        }

        var output = CreateOutputPath(path, isVideo ? ".mp4" : ".mp3");
        var inv = CultureInfo.InvariantCulture;

        var arguments = isVideo
            ? $"-y -i \"{path}\" -c:v libx264 -preset medium -b:v {videoKbps.ToString("0", inv)}k -maxrate {videoKbps.ToString("0", inv)}k -bufsize {(videoKbps * 2).ToString("0", inv)}k " +
              $"-vf \"scale='min(1280,iw)':-2\" -c:a aac -b:a {audioKbps.ToString("0", inv)}k -movflags +faststart \"{output}\""
            : $"-y -i \"{path}\" -vn -c:a libmp3lame -b:a {audioKbps.ToString("0", inv)}k \"{output}\"";

        if (!RunKeepingUiAlive(ffmpeg, arguments) || !File.Exists(output))
        {
            return null;
        }

        return new FileInfo(output).Length <= maxBytes ? output : null;
    }

    private static double? GetDurationSeconds(string path, string ffmpeg)
    {
        var ffprobe = Path.Combine(Path.GetDirectoryName(ffmpeg) ?? "", "ffprobe.exe");

        if (!File.Exists(ffprobe))
        {
            ffprobe = FindTool("ffprobe.exe") ?? "";
        }

        if (!File.Exists(ffprobe))
        {
            return null;
        }

        try
        {
            using var process = Process.Start(new ProcessStartInfo(ffprobe, $"-v error -show_entries format=duration -of csv=p=0 \"{path}\"")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
            });

            if (process == null)
            {
                return null;
            }

            var text = process.StandardOutput.ReadToEnd();
            process.WaitForExit(15_000);

            return double.TryParse(text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds) ? seconds : null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Runs a tool and pumps the UI message loop meanwhile (progress dialog shown), so the window does not freeze.
    /// </summary>
    private static bool RunKeepingUiAlive(string exe, string arguments)
    {
        try
        {
            using var progress = ViewModel.PlatformSpecific.PlatformManager.Instance.ShowProgressDialog();

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo(exe, arguments)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                },
                EnableRaisingEvents = true,
            };

            var frame = new DispatcherFrame();
            process.Exited += (_, _) => frame.Continue = false;

            process.Start();
            process.BeginErrorReadLine(); // FFmpeg writes progress to stderr: read it so the pipe never blocks
            process.BeginOutputReadLine();

            if (!process.HasExited)
            {
                Dispatcher.PushFrame(frame);
            }

            process.WaitForExit();
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Looks for a tool next to the program, in PATH and in the winget links folder.
    /// </summary>
    private static string? FindTool(string fileName)
    {
        var candidates = new List<string> { Path.Combine(AppContext.BaseDirectory, fileName) };

        foreach (var dir in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            candidates.Add(Path.Combine(dir.Trim(), fileName));
        }

        candidates.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "WinGet", "Links", fileName));

        return candidates.FirstOrDefault(File.Exists);
    }

    private static string CreateOutputPath(string source, string extension)
    {
        var dir = Path.Combine(Path.GetTempPath(), AppSettings.ProductName, AppSettings.MediaFolderName, Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, Path.GetFileNameWithoutExtension(source) + extension);
    }
}
