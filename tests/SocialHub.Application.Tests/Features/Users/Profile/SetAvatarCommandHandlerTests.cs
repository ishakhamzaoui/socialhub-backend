using FluentAssertions;
using NSubstitute;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Features.Users.Profile;
using SocialHub.Domain.Media;
using SocialHub.Domain.Users;
using Xunit;
 
namespace SocialHub.Application.Tests.Features.Users.Profile;
 
public class SetAvatarCommandHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IUserProfileRepository _userProfileRepository = Substitute.For<IUserProfileRepository>();
    private readonly IMediaAssetRepository _mediaAssetRepository = Substitute.For<IMediaAssetRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
 
    private SetAvatarCommandHandler CreateHandler() =>
        new(_currentUserService, _userProfileRepository, _mediaAssetRepository, _unitOfWork);
 
    private static MediaAsset ImageAsset(Guid ownerId, MediaCategory category = MediaCategory.User) =>
        MediaAsset.Create(ownerId, MediaKind.Image, category, "avatar.jpg", $"users/{ownerId:N}/x.jpg", null, "image/jpeg", 1024, 400, 400);
 
    [Fact]
    public async Task Handle_Should_Fail_When_AssetIsNotOwnedByRequester()
    {
        var userId = Guid.NewGuid();
        _currentUserService.UserId.Returns(userId.ToString());
        _mediaAssetRepository.GetByIdForOwnerAsync(Arg.Any<Guid>(), userId, Arg.Any<CancellationToken>()).Returns((MediaAsset?)null);
 
        var result = await CreateHandler().Handle(new SetAvatarCommand(Guid.NewGuid()), CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Media.NotFound");
    }
 
    [Fact]
    public async Task Handle_Should_Fail_When_AssetIsAVideo()
    {
        var userId = Guid.NewGuid();
        var asset = MediaAsset.Create(userId, MediaKind.Video, MediaCategory.User, "clip.mp4", $"users/{userId:N}/x.mp4", null, "video/mp4", 1024, null, null, 5);
        _currentUserService.UserId.Returns(userId.ToString());
        _mediaAssetRepository.GetByIdForOwnerAsync(asset.Id, userId, Arg.Any<CancellationToken>()).Returns(asset);
 
        var result = await CreateHandler().Handle(new SetAvatarCommand(asset.Id), CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Media.InvalidKind");
    }
 
    [Fact]
    public async Task Handle_Should_Fail_When_AssetCategoryIsNotUser()
    {
        var userId = Guid.NewGuid();
        var asset = ImageAsset(userId, MediaCategory.Post);
        _currentUserService.UserId.Returns(userId.ToString());
        _mediaAssetRepository.GetByIdForOwnerAsync(asset.Id, userId, Arg.Any<CancellationToken>()).Returns(asset);
 
        var result = await CreateHandler().Handle(new SetAvatarCommand(asset.Id), CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Media.InvalidCategory");
    }
 
    [Fact]
    public async Task Handle_Should_SetAvatarAndKeepPreviousAsHistory_When_Valid()
    {
        var userId = Guid.NewGuid();
        var asset = ImageAsset(userId);
        var profile = UserProfile.CreateDefault(userId, "alice");
        var previousAvatarId = Guid.NewGuid();
        profile.SetAvatar(previousAvatarId);
 
        _currentUserService.UserId.Returns(userId.ToString());
        _mediaAssetRepository.GetByIdForOwnerAsync(asset.Id, userId, Arg.Any<CancellationToken>()).Returns(asset);
        _userProfileRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(profile);
 
        var result = await CreateHandler().Handle(new SetAvatarCommand(asset.Id), CancellationToken.None);
 
        result.IsSuccess.Should().BeTrue();
        profile.AvatarMediaId.Should().Be(asset.Id);
        // Nothing deletes the previous MediaAsset — only the pointer moves.
        _mediaAssetRepository.DidNotReceive().Remove(Arg.Any<MediaAsset>());
    }
}