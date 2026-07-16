using FluentAssertions;
using NSubstitute;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Features.Users.Safety;
using SocialHub.Domain.Users;
using Xunit;
 
namespace SocialHub.Application.Tests.Features.Users.Safety;
 
public class MuteUserCommandHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IUserProfileRepository _userProfileRepository = Substitute.For<IUserProfileRepository>();
    private readonly IUserMuteRepository _userMuteRepository = Substitute.For<IUserMuteRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
 
    private MuteUserCommandHandler CreateHandler() =>
        new(_currentUserService, _userProfileRepository, _userMuteRepository, _unitOfWork);
 
    [Fact]
    public async Task Handle_Should_Fail_When_MutingSelf()
    {
        var userId = Guid.NewGuid();
        _currentUserService.UserId.Returns(userId.ToString());
 
        var result = await CreateHandler().Handle(new MuteUserCommand(userId), CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Mute.CannotMuteSelf");
    }
 
    [Fact]
    public async Task Handle_Should_Succeed_And_NotTouchFollowRepository()
    {
        // Muting has no follow-graph effect (unlike blocking) — this test
        // only asserts the mute row is created; there's no IFollowRepository
        // dependency in this handler at all to assert against, which is
        // itself the point (see UserMute's remarks).
        var requesterId = Guid.NewGuid();
        var targetProfile = UserProfile.CreateDefault(Guid.NewGuid(), "bob");
        _currentUserService.UserId.Returns(requesterId.ToString());
        _userProfileRepository.GetByUserIdAsync(targetProfile.UserId, Arg.Any<CancellationToken>()).Returns(targetProfile);
        _userMuteRepository.IsMutedAsync(requesterId, targetProfile.UserId, Arg.Any<CancellationToken>()).Returns(false);
 
        var result = await CreateHandler().Handle(new MuteUserCommand(targetProfile.UserId), CancellationToken.None);
 
        result.IsSuccess.Should().BeTrue();
        await _userMuteRepository.Received(1).AddAsync(
            Arg.Is<UserMute>(m => m.MuterId == requesterId && m.MutedId == targetProfile.UserId),
            Arg.Any<CancellationToken>());
    }
}