using FluentAssertions;
using NSubstitute;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Features.Authentication.Sessions;
using Xunit;
using DomainRefreshToken = SocialHub.Domain.Users.RefreshToken;
 
namespace SocialHub.Application.Tests.Features.Authentication.Sessions;
 
public class RevokeSessionCommandHandlerTests
{
    private readonly IRepository<DomainRefreshToken, Guid> _refreshTokens = Substitute.For<IRepository<DomainRefreshToken, Guid>>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
 
    private RevokeSessionCommandHandler CreateHandler() => new(_refreshTokens, _unitOfWork, _currentUserService);
 
    [Fact]
    public async Task Handle_Should_Revoke_When_SessionBelongsToCurrentUser()
    {
        var userId = Guid.NewGuid();
        var session = DomainRefreshToken.Create(userId, "hash", DateTime.UtcNow.AddDays(10), "1.2.3.4");
        _currentUserService.UserId.Returns(userId.ToString());
        _refreshTokens.GetByIdAsync(session.Id, Arg.Any<CancellationToken>()).Returns(session);
 
        var handler = CreateHandler();
        var result = await handler.Handle(new RevokeSessionCommand(session.Id), CancellationToken.None);
 
        result.IsSuccess.Should().BeTrue();
        session.IsRevoked.Should().BeTrue();
    }
 
    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_SessionBelongsToAnotherUser()
    {
        var session = DomainRefreshToken.Create(Guid.NewGuid(), "hash", DateTime.UtcNow.AddDays(10), "1.2.3.4");
        _currentUserService.UserId.Returns(Guid.NewGuid().ToString());
        _refreshTokens.GetByIdAsync(session.Id, Arg.Any<CancellationToken>()).Returns(session);
 
        var handler = CreateHandler();
        var result = await handler.Handle(new RevokeSessionCommand(session.Id), CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.SessionNotFound");
    }
}