using FluentAssertions;
using NSubstitute;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Features.Users.Follow;
using SocialHub.Domain.Users;
using Xunit;
 
namespace SocialHub.Application.Tests.Features.Users.Follow;
 
public class UnfollowUserCommandHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IFollowRepository _followRepository = Substitute.For<IFollowRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
 
    private UnfollowUserCommandHandler CreateHandler() => new(_currentUserService, _followRepository, _unitOfWork);
 
    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_NotCurrentlyFollowing()
    {
        var requesterId = Guid.NewGuid();
        _currentUserService.UserId.Returns(requesterId.ToString());
        _followRepository.GetAsync(requesterId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Domain.Users.Follow?)null);
 
        var result = await CreateHandler().Handle(new UnfollowUserCommand(Guid.NewGuid()), CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Follow.NotFound");
    }
 
    [Fact]
    public async Task Handle_Should_RemoveTheFollowRow_When_ItExists()
    {
        var requesterId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var follow = Domain.Users.Follow.Create(requesterId, targetId);
        _currentUserService.UserId.Returns(requesterId.ToString());
        _followRepository.GetAsync(requesterId, targetId, Arg.Any<CancellationToken>()).Returns(follow);
 
        var result = await CreateHandler().Handle(new UnfollowUserCommand(targetId), CancellationToken.None);
 
        result.IsSuccess.Should().BeTrue();
        _followRepository.Received(1).Remove(follow);
    }
}