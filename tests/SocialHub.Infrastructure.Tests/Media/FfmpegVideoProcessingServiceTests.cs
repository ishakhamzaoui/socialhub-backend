using System.Diagnostics;
using FluentAssertions;
using SocialHub.Infrastructure.Media;
using Xunit;
 
namespace SocialHub.Infrastructure.Tests.Media;
 
/// <summary>
/// Real ffprobe/ffmpeg — no mocking, for the same reason as
/// SkiaImageProcessingServiceTests. Synthesizes its own tiny test clip with
/// ffmpeg's "testsrc" lavfi pattern generator in InitializeAsync so no
/// binary video fixture needs to be committed to the repo. Requires ffmpeg
/// on PATH (installed by script 19's apt step).
/// </summary>
public sealed class FfmpegVideoProcessingServiceTests : IAsyncLifetime
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), $"socialhub-video-test-{Guid.NewGuid():N}");
    private readonly FfmpegVideoProcessingService _sut = new();
    private string _testVideoPath = string.Empty;
 
    public async Task InitializeAsync()
    {
        Directory.CreateDirectory(_tempDir);
        _testVideoPath = Path.Combine(_tempDir, "test.mp4");
 
        await RunFfmpegAsync($"-y -f lavfi -i testsrc=duration=2:size=320x240:rate=10 -pix_fmt yuv420p \"{_testVideoPath}\"");
    }
 
    public Task DisposeAsync()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
 
        return Task.CompletedTask;
    }
 
    [Fact]
    public async Task ExtractMetadataAsync_Should_ReturnDurationAndDimensions_ForTheSynthesizedClip()
    {
        var metadata = await _sut.ExtractMetadataAsync(_testVideoPath);
 
        metadata.Width.Should().Be(320);
        metadata.Height.Should().Be(240);
        metadata.DurationSeconds.Should().BeApproximately(2.0, precision: 0.5);
    }
 
    [Fact]
    public async Task GenerateThumbnailAsync_Should_ProduceANonEmptyJpegFile()
    {
        var thumbnailPath = Path.Combine(_tempDir, "thumb.jpg");
 
        await _sut.GenerateThumbnailAsync(_testVideoPath, thumbnailPath);
 
        File.Exists(thumbnailPath).Should().BeTrue();
        new FileInfo(thumbnailPath).Length.Should().BeGreaterThan(0);
    }
 
    [Fact]
    public async Task ExtractMetadataAsync_Should_Throw_When_FileDoesNotExist()
    {
        var act = async () => await _sut.ExtractMetadataAsync(Path.Combine(_tempDir, "missing.mp4"));
 
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
 
    private static async Task RunFfmpegAsync(string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
 
        using var process = new Process { StartInfo = startInfo };
        process.Start();
        await process.WaitForExitAsync();
 
        if (process.ExitCode != 0)
        {
            var stdErr = await process.StandardError.ReadToEndAsync();
            throw new InvalidOperationException($"Failed to synthesize the test video fixture: {stdErr}");
        }
    }
}