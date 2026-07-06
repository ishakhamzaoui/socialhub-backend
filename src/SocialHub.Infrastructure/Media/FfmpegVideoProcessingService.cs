using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using SocialHub.Application.Common.Interfaces;
 
namespace SocialHub.Infrastructure.Media;
 
/// <summary>
/// Video metadata extraction (roadmap 4.5) and thumbnail-frame generation
/// (roadmap 4.6) by shelling out to the `ffprobe`/`ffmpeg` binaries that must
/// be installed on the host (apt package `ffmpeg`), via
/// System.Diagnostics.Process — not a NuGet dependency on FFmpeg itself.
/// Nothing is linked into or redistributed with SocialHub, so this sidesteps
/// FFmpeg's build-configuration-dependent LGPL/GPL licensing question
/// entirely; it's invoked the same way the deployment scripts already invoke
/// `pg_dump`/`rsync` (spec §31).
/// </summary>
public sealed class FfmpegVideoProcessingService : IVideoProcessingService
{
    public async Task<VideoMetadata> ExtractMetadataAsync(string absoluteSourcePath, CancellationToken cancellationToken = default)
    {
        var arguments = $"-v error -print_format json -show_format -show_streams \"{absoluteSourcePath}\"";
        var (exitCode, stdOut, stdErr) = await RunProcessAsync("ffprobe", arguments, cancellationToken);
 
        if (exitCode != 0)
        {
            throw new InvalidOperationException($"ffprobe failed (exit {exitCode}): {stdErr}");
        }
 
        using var document = JsonDocument.Parse(stdOut);
        var root = document.RootElement;
 
        var durationSeconds = 0d;
        if (root.TryGetProperty("format", out var format) &&
            format.TryGetProperty("duration", out var durationProperty) &&
            double.TryParse(durationProperty.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedDuration))
        {
            durationSeconds = parsedDuration;
        }
 
        var width = 0;
        var height = 0;
        if (root.TryGetProperty("streams", out var streams))
        {
            foreach (var stream in streams.EnumerateArray())
            {
                if (stream.TryGetProperty("codec_type", out var codecType) && codecType.GetString() == "video")
                {
                    width = stream.TryGetProperty("width", out var widthProperty) ? widthProperty.GetInt32() : 0;
                    height = stream.TryGetProperty("height", out var heightProperty) ? heightProperty.GetInt32() : 0;
                    break;
                }
            }
        }
 
        return new VideoMetadata(durationSeconds, width, height);
    }
 
    public async Task GenerateThumbnailAsync(string absoluteSourcePath, string absoluteDestinationPath, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(absoluteDestinationPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
 
        // Seek 1s in to skip a likely-black first frame; very short clips fall
        // back to grabbing the first frame outright.
        var arguments = $"-y -ss 00:00:01 -i \"{absoluteSourcePath}\" -frames:v 1 -q:v 3 \"{absoluteDestinationPath}\"";
        var (exitCode, _, stdErr) = await RunProcessAsync("ffmpeg", arguments, cancellationToken);
 
        if (exitCode != 0 || !File.Exists(absoluteDestinationPath))
        {
            arguments = $"-y -i \"{absoluteSourcePath}\" -frames:v 1 -q:v 3 \"{absoluteDestinationPath}\"";
            (exitCode, _, stdErr) = await RunProcessAsync("ffmpeg", arguments, cancellationToken);
 
            if (exitCode != 0)
            {
                throw new InvalidOperationException($"ffmpeg thumbnail extraction failed (exit {exitCode}): {stdErr}");
            }
        }
    }
 
    private static async Task<(int ExitCode, string StdOut, string StdErr)> RunProcessAsync(string fileName, string arguments, CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
 
        using var process = new Process { StartInfo = startInfo };
 
        try
        {
            process.Start();
        }
        catch (System.ComponentModel.Win32Exception ex)
        {
            throw new InvalidOperationException(
                $"Unable to start '{fileName}'. Ensure the ffmpeg package is installed on the host (sudo apt install ffmpeg).", ex);
        }
 
        var stdOutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stdErrTask = process.StandardError.ReadToEndAsync(cancellationToken);
 
        await process.WaitForExitAsync(cancellationToken);
 
        return (process.ExitCode, await stdOutTask, await stdErrTask);
    }
}