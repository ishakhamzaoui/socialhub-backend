using FluentAssertions;
using NSubstitute;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Features.Media;
using SocialHub.Domain.Media;
using Xunit;
 
namespace SocialHub.Application.Tests.Features.Media;
 
public class GetMediaQueryHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IMediaAssetRepository _repository = Substitute.For<IMediaAssetRepository>();
 
    private GetMediaQueryHandler CreateHandler() => new(_currentUserService, _repository);
 
    [Fact]
    public async Task Handle_Should_ReturnUnauthorized_When_CurrentUserIdIsNotAValidGuid()
    {
        _currentUserService.UserId.Returns((string?)null);
 
        var handler = CreateHandler();
        var result = await handler.Handle(new GetMediaQuery(Guid.NewGuid()), CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.NotAuthenticated");
    }
 
    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_AssetDoesNotExistForThisOwner()
    {
        var ownerId = Guid.NewGuid();
        _currentUserService.UserId.Returns(ownerId.ToString());
        _repository.GetByIdForOwnerAsync(Arg.Any<Guid>(), ownerId, Arg.Any<CancellationToken>())
            .Returns((MediaAsset?)null);
 
        var handler = CreateHandler();
        var result = await handler.Handle(new GetMediaQuery(Guid.NewGuid()), CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Media.NotFound");
    }
 
    [Fact]
    public async Task Handle_Should_ReturnMediaDto_When_AssetExistsForThisOwner()
    {
        var ownerId = Guid.NewGuid();
        _currentUserService.UserId.Returns(ownerId.ToString());
 
        var asset = MediaAsset.Create(
            ownerId, MediaKind.Image, MediaCategory.User, "avatar.jpg",
            $"users/{ownerId:N}/x.jpg", $"thumbnails/x.jpg", "image/jpeg", 1024, 400, 400);
 
        _repository.GetByIdForOwnerAsync(asset.Id, ownerId, Arg.Any<CancellationToken>()).Returns(asset);
 
        var handler = CreateHandler();
        var result = await handler.Handle(new GetMediaQuery(asset.Id), CancellationToken.None);
 
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(asset.Id);
        result.Value.OriginalFileName.Should().Be("avatar.jpg");
    }
}