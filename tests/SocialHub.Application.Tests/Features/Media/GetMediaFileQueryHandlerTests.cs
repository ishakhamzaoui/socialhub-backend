using FluentAssertions;
using NSubstitute;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Features.Media;
using SocialHub.Domain.Media;
using Xunit;
 
namespace SocialHub.Application.Tests.Features.Media;
 
public class GetMediaFileQueryHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IMediaAssetRepository _repository = Substitute.For<IMediaAssetRepository>();
 
    private GetMediaFileQueryHandler CreateHandler() => new(_currentUserService, _repository);
 
    private MediaAsset CreateAssetWithThumbnail(Guid ownerId) => MediaAsset.Create(
        ownerId, MediaKind.Image, MediaCategory.User, "avatar.jpg",
        $"users/{ownerId:N}/x.jpg", $"thumbnails/x.jpg", "image/jpeg", 1024, 400, 400);
 
    [Fact]
    public async Task Handle_Should_ReturnMainFile_When_ThumbnailNotRequested()
    {
        var ownerId = Guid.NewGuid();
        _currentUserService.UserId.Returns(ownerId.ToString());
        var asset = CreateAssetWithThumbnail(ownerId);
        _repository.GetByIdForOwnerAsync(asset.Id, ownerId, Arg.Any<CancellationToken>()).Returns(asset);
 
        var handler = CreateHandler();
        var result = await handler.Handle(new GetMediaFileQuery(asset.Id), CancellationToken.None);
 
        result.IsSuccess.Should().BeTrue();
        result.Value.RelativePath.Should().Be(asset.StoragePath);
        result.Value.MimeType.Should().Be(asset.MimeType);
        result.Value.FileName.Should().Be(asset.OriginalFileName);
    }
 
    [Fact]
    public async Task Handle_Should_ReturnThumbnailFile_When_ThumbnailRequestedAndPresent()
    {
        var ownerId = Guid.NewGuid();
        _currentUserService.UserId.Returns(ownerId.ToString());
        var asset = CreateAssetWithThumbnail(ownerId);
        _repository.GetByIdForOwnerAsync(asset.Id, ownerId, Arg.Any<CancellationToken>()).Returns(asset);
 
        var handler = CreateHandler();
        var result = await handler.Handle(new GetMediaFileQuery(asset.Id, Thumbnail: true), CancellationToken.None);
 
        result.IsSuccess.Should().BeTrue();
        result.Value.RelativePath.Should().Be(asset.ThumbnailStoragePath);
        result.Value.MimeType.Should().Be("image/jpeg");
        result.Value.FileName.Should().Be($"thumb-{asset.OriginalFileName}");
    }
 
    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_ThumbnailRequestedButNoneExists()
    {
        var ownerId = Guid.NewGuid();
        _currentUserService.UserId.Returns(ownerId.ToString());
 
        var asset = MediaAsset.Create(
            ownerId, MediaKind.Other, MediaCategory.Message, "file.bin",
            $"messages/{ownerId:N}/x.bin", null, "application/octet-stream", 1024);
 
        _repository.GetByIdForOwnerAsync(asset.Id, ownerId, Arg.Any<CancellationToken>()).Returns(asset);
 
        var handler = CreateHandler();
        var result = await handler.Handle(new GetMediaFileQuery(asset.Id, Thumbnail: true), CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Media.ThumbnailNotFound");
    }
}