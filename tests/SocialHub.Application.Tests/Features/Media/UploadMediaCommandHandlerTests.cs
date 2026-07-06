using FluentAssertions;
using NSubstitute;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Features.Media;
using SocialHub.Domain.Media;
using Xunit;
 
namespace SocialHub.Application.Tests.Features.Media;
 
public class UploadMediaCommandHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IFileStorageService _fileStorageService = Substitute.For<IFileStorageService>();
    private readonly IImageProcessingService _imageProcessingService = Substitute.For<IImageProcessingService>();
    private readonly IVideoProcessingService _videoProcessingService = Substitute.For<IVideoProcessingService>();
    private readonly IMediaAssetRepository _mediaAssetRepository = Substitute.For<IMediaAssetRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
 
    private UploadMediaCommandHandler CreateHandler() => new(
        _currentUserService, _fileStorageService, _imageProcessingService, _videoProcessingService, _mediaAssetRepository, _unitOfWork);
 
    private void SetUpAuthenticatedUser(Guid userId) => _currentUserService.UserId.Returns(userId.ToString());
 
    [Fact]
    public async Task Handle_Should_ReturnUnauthorized_When_CurrentUserIdIsNotAValidGuid()
    {
        _currentUserService.UserId.Returns((string?)null);
 
        var handler = CreateHandler();
        var command = new UploadMediaCommand(Stream.Null, "photo.jpg", "image/jpeg", 1024, MediaCategory.Post);
 
        var result = await handler.Handle(command, CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.NotAuthenticated");
    }
 
    [Fact]
    public async Task Handle_Should_ProcessAndPersistAnImage_When_MimeTypeIsImage()
    {
        var ownerId = Guid.NewGuid();
        SetUpAuthenticatedUser(ownerId);
 
        _fileStorageService.SaveToTempAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("temp/original.jpg");
        _imageProcessingService.ResizeAndCompressAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new ImageDimensions(800, 600));
        _fileStorageService.PromoteAsync(
                Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<Guid>(), Arg.Any<MediaCategory>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(($"posts/{ownerId:N}/final.jpg", (string?)"thumbnails/final-thumb.jpg"));
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
 
        var handler = CreateHandler();
        var command = new UploadMediaCommand(new MemoryStream([1, 2, 3]), "photo.jpg", "image/jpeg", 2048, MediaCategory.Post);
 
        var result = await handler.Handle(command, CancellationToken.None);
 
        result.IsSuccess.Should().BeTrue();
        result.Value.Kind.Should().Be(MediaKind.Image);
        result.Value.Category.Should().Be(MediaCategory.Post);
        result.Value.WidthPx.Should().Be(800);
        result.Value.HeightPx.Should().Be(600);
        result.Value.ThumbnailUrl.Should().Be($"/api/v1/media/{result.Value.Id}/thumbnail");
        result.Value.Url.Should().Be($"/api/v1/media/{result.Value.Id}/download");
 
        await _mediaAssetRepository.Received(1).AddAsync(Arg.Any<MediaAsset>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
 
    [Fact]
    public async Task Handle_Should_ExtractMetadataAndPersistAVideo_When_MimeTypeIsVideo()
    {
        var ownerId = Guid.NewGuid();
        SetUpAuthenticatedUser(ownerId);
 
        _fileStorageService.SaveToTempAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("temp/original.mp4");
        _videoProcessingService.ExtractMetadataAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new VideoMetadata(12.5, 1920, 1080));
        _fileStorageService.PromoteAsync(
                Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<Guid>(), Arg.Any<MediaCategory>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(($"posts/{ownerId:N}/final.mp4", (string?)"thumbnails/final-thumb.jpg"));
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
 
        var handler = CreateHandler();
        var command = new UploadMediaCommand(new MemoryStream([1, 2, 3]), "clip.mp4", "video/mp4", 4096, MediaCategory.Post);
 
        var result = await handler.Handle(command, CancellationToken.None);
 
        result.IsSuccess.Should().BeTrue();
        result.Value.Kind.Should().Be(MediaKind.Video);
        result.Value.DurationSeconds.Should().Be(12.5);
        result.Value.WidthPx.Should().Be(1920);
        result.Value.HeightPx.Should().Be(1080);
    }
 
    [Fact]
    public async Task Handle_Should_DeleteTempFilesAndRethrow_When_ProcessingFails()
    {
        var ownerId = Guid.NewGuid();
        SetUpAuthenticatedUser(ownerId);
 
        _fileStorageService.SaveToTempAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("temp/original.jpg");
        _imageProcessingService.ResizeAndCompressAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ImageDimensions>(new InvalidOperationException("Simulated processing failure.")));
            // .Returns(_ => throw new InvalidOperationException("Simulated processing failure."));
 
        var handler = CreateHandler();
        var command = new UploadMediaCommand(new MemoryStream([1, 2, 3]), "photo.jpg", "image/jpeg", 2048, MediaCategory.Post);
 
        var act = async () => await handler.Handle(command, CancellationToken.None);
 
        await act.Should().ThrowAsync<InvalidOperationException>();
        await _fileStorageService.Received(1).DeleteAsync("temp/original.jpg", Arg.Any<CancellationToken>());
        await _mediaAssetRepository.DidNotReceive().AddAsync(Arg.Any<MediaAsset>(), Arg.Any<CancellationToken>());
    }
}