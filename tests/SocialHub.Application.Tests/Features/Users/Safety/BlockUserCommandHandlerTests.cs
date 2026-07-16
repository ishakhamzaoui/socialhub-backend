using FluentAssertions;
using NSubstitute;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Features.Users.Safety;
using SocialHub.Domain.Users;
using Xunit;
 
namespace SocialHub.Application.Tests.Features.Users.Safety;
 
public class BlockUserCommandHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IUserProfileRepository _userProfileRepository = Substitute.For<IUserProfileRepository>();
    private readonly IUserBlockRepository _userBlockRepository = Substitute.For<IUserBlockRepository>();
    private readonly IFollowRepository _followRepository = Substitute.For<IFollowRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
 
    private BlockUserCommandHandler CreateHandler() =>
        new(_currentUserService, _userProfileRepository, _userBlockRepository, _followRepository, _unitOfWork);
 
    [Fact]
    public async Task Handle_Should_Fail_When_BlockingSelf()
    {
        var userId = Guid.NewGuid();
        _currentUserService.UserId.Returns(userId.ToString());
 
        var result = await CreateHandler().Handle(new BlockUserCommand(userId), CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Block.CannotBlockSelf");
    }
 
    [Fact]
    public async Task Handle_Should_ReturnConflict_When_AlreadyBlocked()
    {
        var requesterId = Guid.NewGuid();
        var targetProfile = UserProfile.CreateDefault(Guid.NewGuid(), "bob");
        _currentUserService.UserId.Returns(requesterId.ToString());
        _userProfileRepository.GetByUserIdAsync(targetProfile.UserId, Arg.Any<CancellationToken>()).Returns(targetProfile);
        _userBlockRepository.IsBlockedAsync(requesterId, targetProfile.UserId, Arg.Any<CancellationToken>()).Returns(true);
 
        var result = await CreateHandler().Handle(new BlockUserCommand(targetProfile.UserId), CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Block.AlreadyBlocked");
    }
 
    [Fact]
    public async Task Handle_Should_CreateBlockAndRemoveAnyExistingFollow_When_Valid()
    {
        var requesterId = Guid.NewGuid();
        var targetProfile = UserProfile.CreateDefault(Guid.NewGuid(), "bob");
        _currentUserService.UserId.Returns(requesterId.ToString());
        _userProfileRepository.GetByUserIdAsync(targetProfile.UserId, Arg.Any<CancellationToken>()).Returns(targetProfile);
        _userBlockRepository.IsBlockedAsync(requesterId, targetProfile.UserId, Arg.Any<CancellationToken>()).Returns(false);
 
        var result = await CreateHandler().Handle(new BlockUserCommand(targetProfile.UserId), CancellationToken.None);
 
        result.IsSuccess.Should().BeTrue();
        await _userBlockRepository.Received(1).AddAsync(
            Arg.Is<UserBlock>(b => b.BlockerId == requesterId && b.BlockedId == targetProfile.UserId),
            Arg.Any<CancellationToken>());
        await _followRepository.Received(1).RemoveBetweenAsync(requesterId, targetProfile.UserId, Arg.Any<CancellationToken>());
    }
}