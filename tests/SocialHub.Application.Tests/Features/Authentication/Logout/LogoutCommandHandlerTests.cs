using FluentAssertions;
using NSubstitute;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Specifications;
using SocialHub.Application.Features.Authentication.Logout;
using Xunit;
using DomainRefreshToken = SocialHub.Domain.Users.RefreshToken;
 
namespace SocialHub.Application.Tests.Features.Authentication.Logout;
 
public class LogoutCommandHandlerTests
{
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly IRepository<DomainRefreshToken, Guid> _refreshTokens = Substitute.For<IRepository<DomainRefreshToken, Guid>>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
 
    private LogoutCommandHandler CreateHandler() => new(_tokenService, _refreshTokens, _unitOfWork, _currentUserService);
 
    [Fact]
    public async Task Handle_Should_RevokeToken_When_OwnedByCurrentUser()
    {
        var userId = Guid.NewGuid();
        var token = DomainRefreshToken.Create(userId, "hash", DateTime.UtcNow.AddDays(10), "1.2.3.4");
 
        _tokenService.HashToken("raw").Returns("hash");
        _refreshTokens.FirstOrDefaultAsync(Arg.Any<ISpecification<DomainRefreshToken>>(), Arg.Any<CancellationToken>()).Returns(token);
        _currentUserService.UserId.Returns(userId.ToString());
 
        var handler = CreateHandler();
        var result = await handler.Handle(new LogoutCommand("raw"), CancellationToken.None);
 
        result.IsSuccess.Should().BeTrue();
        token.IsRevoked.Should().BeTrue();
    }
 
    [Fact]
    public async Task Handle_Should_ReturnForbidden_When_TokenBelongsToAnotherUser()
    {
        var token = DomainRefreshToken.Create(Guid.NewGuid(), "hash", DateTime.UtcNow.AddDays(10), "1.2.3.4");
 
        _tokenService.HashToken("raw").Returns("hash");
        _refreshTokens.FirstOrDefaultAsync(Arg.Any<ISpecification<DomainRefreshToken>>(), Arg.Any<CancellationToken>()).Returns(token);
        _currentUserService.UserId.Returns(Guid.NewGuid().ToString());
 
        var handler = CreateHandler();
        var result = await handler.Handle(new LogoutCommand("raw"), CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.TokenOwnershipMismatch");
    }
 
    [Fact]
    public async Task Handle_Should_ReturnSuccess_When_TokenDoesNotExist()
    {
        _tokenService.HashToken("raw").Returns("hash");
        _refreshTokens.FirstOrDefaultAsync(Arg.Any<ISpecification<DomainRefreshToken>>(), Arg.Any<CancellationToken>())
            .Returns((DomainRefreshToken?)null);
 
        var handler = CreateHandler();
        var result = await handler.Handle(new LogoutCommand("raw"), CancellationToken.None);
 
        result.IsSuccess.Should().BeTrue();
    }
}