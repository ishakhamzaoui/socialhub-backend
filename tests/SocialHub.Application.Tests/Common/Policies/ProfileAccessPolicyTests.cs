using FluentAssertions;
using NSubstitute;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Policies;
using SocialHub.Domain.Users;
using Xunit;
 
namespace SocialHub.Application.Tests.Common.Policies;
 
public class ProfileAccessPolicyTests
{
    private readonly IFollowRepository _followRepository = Substitute.For<IFollowRepository>();
    private readonly IUserBlockRepository _userBlockRepository = Substitute.For<IUserBlockRepository>();
 
    [Fact]
    public async Task EvaluateAsync_Should_ReturnOwner_When_RequesterIsTheProfileOwner()
    {
        var profile = UserProfile.CreateDefault(Guid.NewGuid(), "alice");
        profile.UpdateVisibility(ProfileVisibility.Private);
 
        var result = await ProfileAccessPolicy.EvaluateAsync(_followRepository, _userBlockRepository, profile, profile.UserId);
 
        result.Should().Be(ProfileAccessResult.Owner);
    }
 
    [Fact]
    public async Task EvaluateAsync_Should_ReturnBlocked_When_EitherPartyHasBlockedTheOther()
    {
        var profile = UserProfile.CreateDefault(Guid.NewGuid(), "alice");
        var requesterId = Guid.NewGuid();
        _userBlockRepository.IsBlockedEitherDirectionAsync(profile.UserId, requesterId, Arg.Any<CancellationToken>()).Returns(true);
 
        var result = await ProfileAccessPolicy.EvaluateAsync(_followRepository, _userBlockRepository, profile, requesterId);
 
        result.Should().Be(ProfileAccessResult.Blocked);
    }
 
    [Fact]
    public async Task EvaluateAsync_Should_ReturnAllowed_When_ProfileIsPublic()
    {
        var profile = UserProfile.CreateDefault(Guid.NewGuid(), "alice");
 
        var result = await ProfileAccessPolicy.EvaluateAsync(_followRepository, _userBlockRepository, profile, Guid.NewGuid());
 
        result.Should().Be(ProfileAccessResult.Allowed);
    }
 
    [Fact]
    public async Task EvaluateAsync_Should_ReturnDenied_When_ProfileIsPrivateAndRequesterIsNotOwner()
    {
        var profile = UserProfile.CreateDefault(Guid.NewGuid(), "alice");
        profile.UpdateVisibility(ProfileVisibility.Private);
 
        var result = await ProfileAccessPolicy.EvaluateAsync(_followRepository, _userBlockRepository, profile, Guid.NewGuid());
 
        result.Should().Be(ProfileAccessResult.Denied);
    }
 
    [Fact]
    public async Task EvaluateAsync_Should_ReturnAllowed_When_FollowersOnlyAndRequesterFollows()
    {
        var profile = UserProfile.CreateDefault(Guid.NewGuid(), "alice");
        profile.UpdateVisibility(ProfileVisibility.FollowersOnly);
        var requesterId = Guid.NewGuid();
        _followRepository.ExistsAsync(requesterId, profile.UserId, Arg.Any<CancellationToken>()).Returns(true);
 
        var result = await ProfileAccessPolicy.EvaluateAsync(_followRepository, _userBlockRepository, profile, requesterId);
 
        result.Should().Be(ProfileAccessResult.Allowed);
    }
 
    [Fact]
    public async Task EvaluateAsync_Should_ReturnDenied_When_FollowersOnlyAndRequesterDoesNotFollow()
    {
        var profile = UserProfile.CreateDefault(Guid.NewGuid(), "alice");
        profile.UpdateVisibility(ProfileVisibility.FollowersOnly);
        var requesterId = Guid.NewGuid();
        _followRepository.ExistsAsync(requesterId, profile.UserId, Arg.Any<CancellationToken>()).Returns(false);
 
        var result = await ProfileAccessPolicy.EvaluateAsync(_followRepository, _userBlockRepository, profile, requesterId);
 
        result.Should().Be(ProfileAccessResult.Denied);
    }
}