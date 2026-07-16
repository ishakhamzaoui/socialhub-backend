using FluentAssertions;
using NSubstitute;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Features.Users.Follow;
using SocialHub.Domain.Users;
using Xunit;
 
namespace SocialHub.Application.Tests.Features.Users.Follow;
 
public class FollowUserCommandHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IUserProfileRepository _userProfileRepository = Substitute.For<IUserProfileRepository>();
    private readonly IFollowRepository _followRepository = Substitute.For<IFollowRepository>();
    private readonly IUserBlockRepository _userBlockRepository = Substitute.For<IUserBlockRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
 
    private FollowUserCommandHandler CreateHandler() =>
        new(_currentUserService, _userProfileRepository, _followRepository, _userBlockRepository, _unitOfWork);
 
    [Fact]
    public async Task Handle_Should_Fail_When_FollowingSelf()
    {
        var userId = Guid.NewGuid();
        _currentUserService.UserId.Returns(userId.ToString());
 
        var result = await CreateHandler().Handle(new FollowUserCommand(userId), CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Follow.CannotFollowSelf");
    }
 
    [Fact]
    public async Task Handle_Should_ReturnNotFound_NotForbidden_When_Blocked()
    {
        var requesterId = Guid.NewGuid();
        var targetProfile = UserProfile.CreateDefault(Guid.NewGuid(), "bob");
        _currentUserService.UserId.Returns(requesterId.ToString());
        _userProfileRepository.GetByUserIdAsync(targetProfile.UserId, Arg.Any<CancellationToken>()).Returns(targetProfile);
        _userBlockRepository.IsBlockedEitherDirectionAsync(requesterId, targetProfile.UserId, Arg.Any<CancellationToken>()).Returns(true);
 
        var result = await CreateHandler().Handle(new FollowUserCommand(targetProfile.UserId), CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.NotFound");
    }
 
    [Fact]
    public async Task Handle_Should_ReturnConflict_When_AlreadyFollowing()
    {
        var requesterId = Guid.NewGuid();
        var targetProfile = UserProfile.CreateDefault(Guid.NewGuid(), "bob");
        _currentUserService.UserId.Returns(requesterId.ToString());
        _userProfileRepository.GetByUserIdAsync(targetProfile.UserId, Arg.Any<CancellationToken>()).Returns(targetProfile);
        _userBlockRepository.IsBlockedEitherDirectionAsync(requesterId, targetProfile.UserId, Arg.Any<CancellationToken>()).Returns(false);
        _followRepository.ExistsAsync(requesterId, targetProfile.UserId, Arg.Any<CancellationToken>()).Returns(true);
 
        var result = await CreateHandler().Handle(new FollowUserCommand(targetProfile.UserId), CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Follow.AlreadyFollowing");
    }
 
    [Fact]
    public async Task Handle_Should_Succeed_When_ValidNewFollow()
    {
        var requesterId = Guid.NewGuid();
        var targetProfile = UserProfile.CreateDefault(Guid.NewGuid(), "bob");
        _currentUserService.UserId.Returns(requesterId.ToString());
        _userProfileRepository.GetByUserIdAsync(targetProfile.UserId, Arg.Any<CancellationToken>()).Returns(targetProfile);
        _userBlockRepository.IsBlockedEitherDirectionAsync(requesterId, targetProfile.UserId, Arg.Any<CancellationToken>()).Returns(false);
        _followRepository.ExistsAsync(requesterId, targetProfile.UserId, Arg.Any<CancellationToken>()).Returns(false);
 
        var result = await CreateHandler().Handle(new FollowUserCommand(targetProfile.UserId), CancellationToken.None);
 
        result.IsSuccess.Should().BeTrue();
        await _followRepository.Received(1).AddAsync(
            Arg.Is<Domain.Users.Follow>(f => f.FollowerId == requesterId && f.FollowingId == targetProfile.UserId),
            Arg.Any<CancellationToken>());
    }
}