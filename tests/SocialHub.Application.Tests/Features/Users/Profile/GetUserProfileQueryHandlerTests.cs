using FluentAssertions;
using NSubstitute;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Features.Users.Profile;
using SocialHub.Domain.Users;
using Xunit;
 
namespace SocialHub.Application.Tests.Features.Users.Profile;
 
public class GetUserProfileQueryHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IUserProfileRepository _userProfileRepository = Substitute.For<IUserProfileRepository>();
    private readonly IFollowRepository _followRepository = Substitute.For<IFollowRepository>();
    private readonly IUserBlockRepository _userBlockRepository = Substitute.For<IUserBlockRepository>();
 
    private GetUserProfileQueryHandler CreateHandler() =>
        new(_currentUserService, _userProfileRepository, _followRepository, _userBlockRepository);
 
    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_TargetHasNoProfile()
    {
        var requesterId = Guid.NewGuid();
        _currentUserService.UserId.Returns(requesterId.ToString());
        _userProfileRepository.GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((UserProfile?)null);
 
        var result = await CreateHandler().Handle(new GetUserProfileQuery(Guid.NewGuid()), CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Profile.NotFound");
    }
 
    [Fact]
    public async Task Handle_Should_ReturnNotFound_NotForbidden_When_Blocked()
    {
        var requesterId = Guid.NewGuid();
        var profile = UserProfile.CreateDefault(Guid.NewGuid(), "alice");
        _currentUserService.UserId.Returns(requesterId.ToString());
        _userProfileRepository.GetByUserIdAsync(profile.UserId, Arg.Any<CancellationToken>()).Returns(profile);
        _userBlockRepository.IsBlockedEitherDirectionAsync(profile.UserId, requesterId, Arg.Any<CancellationToken>()).Returns(true);
 
        var result = await CreateHandler().Handle(new GetUserProfileQuery(profile.UserId), CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Profile.NotFound"); // never Forbidden — block existence must not leak
    }
 
    [Fact]
    public async Task Handle_Should_ReturnForbidden_When_ProfileIsPrivateAndNotFollowed()
    {
        var requesterId = Guid.NewGuid();
        var profile = UserProfile.CreateDefault(Guid.NewGuid(), "alice");
        profile.UpdateVisibility(ProfileVisibility.Private);
        _currentUserService.UserId.Returns(requesterId.ToString());
        _userProfileRepository.GetByUserIdAsync(profile.UserId, Arg.Any<CancellationToken>()).Returns(profile);
 
        var result = await CreateHandler().Handle(new GetUserProfileQuery(profile.UserId), CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Profile.Private");
    }
 
    [Fact]
    public async Task Handle_Should_ReturnDto_When_ProfileIsPublic()
    {
        var requesterId = Guid.NewGuid();
        var profile = UserProfile.CreateDefault(Guid.NewGuid(), "alice");
        _currentUserService.UserId.Returns(requesterId.ToString());
        _userProfileRepository.GetByUserIdAsync(profile.UserId, Arg.Any<CancellationToken>()).Returns(profile);
 
        var result = await CreateHandler().Handle(new GetUserProfileQuery(profile.UserId), CancellationToken.None);
 
        result.IsSuccess.Should().BeTrue();
        result.Value.DisplayName.Should().Be("alice");
        result.Value.IsOwnProfile.Should().BeFalse();
    }
}