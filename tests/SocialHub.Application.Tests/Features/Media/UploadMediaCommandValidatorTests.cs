using FluentAssertions;
using SocialHub.Application.Features.Media;
using SocialHub.Domain.Media;
using Xunit;
 
namespace SocialHub.Application.Tests.Features.Media;
 
public class UploadMediaCommandValidatorTests
{
    private readonly UploadMediaCommandValidator _validator = new();
 
    private static UploadMediaCommand Command(string mimeType, long sizeBytes, string fileName = "file.bin") =>
        new(Stream.Null, fileName, mimeType, sizeBytes, MediaCategory.Post);
 
    [Fact]
    public void Should_Pass_ForASmallJpegImage()
    {
        var result = _validator.Validate(Command("image/jpeg", 1024 * 1024, "photo.jpg"));
 
        result.IsValid.Should().BeTrue();
    }
 
    [Fact]
    public void Should_Pass_ForAnMp4VideoUnderTheSizeLimit()
    {
        var result = _validator.Validate(Command("video/mp4", 100L * 1024 * 1024, "clip.mp4"));
 
        result.IsValid.Should().BeTrue();
    }
 
    [Fact]
    public void Should_Fail_When_OriginalFileNameIsEmpty()
    {
        var result = _validator.Validate(Command("image/jpeg", 1024, fileName: string.Empty));
 
        result.IsValid.Should().BeFalse();
    }
 
    [Fact]
    public void Should_Fail_When_SizeBytesIsZero()
    {
        var result = _validator.Validate(Command("image/jpeg", 0));
 
        result.IsValid.Should().BeFalse();
    }
 
    [Fact]
    public void Should_Fail_When_MimeTypeIsNotAnAllowedImageOrVideoType()
    {
        var result = _validator.Validate(Command("application/pdf", 1024, "doc.pdf"));
 
        result.IsValid.Should().BeFalse();
    }
 
    [Fact]
    public void Should_Fail_When_ImageExceedsTheImageSizeLimit()
    {
        var result = _validator.Validate(Command("image/jpeg", 16L * 1024 * 1024, "huge.jpg"));
 
        result.IsValid.Should().BeFalse();
    }
 
    [Fact]
    public void Should_Fail_When_VideoExceedsTheVideoSizeLimit()
    {
        var result = _validator.Validate(Command("video/mp4", 600L * 1024 * 1024, "huge.mp4"));
 
        result.IsValid.Should().BeFalse();
    }
}