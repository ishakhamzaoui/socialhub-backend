using FluentAssertions;
using NSubstitute;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Models;
using SocialHub.Application.Common.Results;
using SocialHub.Application.Common.Specifications;
using SocialHub.Application.Features.Authentication.RefreshToken;
using Xunit;
using DomainRefreshToken = SocialHub.Domain.Users.RefreshToken;
 
namespace SocialHub.Application.Tests.Features.Authentication.RefreshToken;
 
public class RefreshTokenCommandHandlerTests
{
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly IRepository<DomainRefreshToken, Guid> _refreshTokens = Substitute.For<IRepository<DomainRefreshToken, Guid>>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
 
    private RefreshTokenCommandHandler CreateHandler() => new(_tokenService, _identityService, _refreshTokens, _unitOfWork);
 
    [Fact]
    public async Task Handle_Should_ReturnFailure_When_TokenNotFound()
    {
        _tokenService.HashToken("raw").Returns("hash");
        _refreshTokens.FirstOrDefaultAsync(Arg.Any<ISpecification<DomainRefreshToken>>(), Arg.Any<CancellationToken>())
            .Returns((DomainRefreshToken?)null);
 
        var handler = CreateHandler();
        var result = await handler.Handle(new RefreshTokenCommand("raw", "127.0.0.1"), CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.InvalidRefreshToken");
    }
 
    [Fact]
    public async Task Handle_Should_RevokeFamily_When_ReuseIsDetected()
    {
        var userId = Guid.NewGuid();
        var revoked = DomainRefreshToken.Create(userId, "old-hash", DateTime.UtcNow.AddDays(10), "1.2.3.4");
        revoked.Revoke("1.2.3.4", "some-new-hash"); // already redeemed once — presenting it again is reuse
 
        _tokenService.HashToken("raw").Returns("old-hash");
        _refreshTokens.FirstOrDefaultAsync(Arg.Any<ISpecification<DomainRefreshToken>>(), Arg.Any<CancellationToken>())
            .Returns(revoked);
        _refreshTokens.ListAsync(Arg.Any<ISpecification<DomainRefreshToken>>(), Arg.Any<CancellationToken>())
            .Returns(new List<DomainRefreshToken> { revoked });
 
        var handler = CreateHandler();
        var result = await handler.Handle(new RefreshTokenCommand("raw", "9.9.9.9"), CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.RefreshTokenReuseDetected");
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
 
    [Fact]
    public async Task Handle_Should_ReturnFailure_When_TokenIsExpired()
    {
        var expired = DomainRefreshToken.Create(Guid.NewGuid(), "hash", DateTime.UtcNow.AddDays(-1), "1.2.3.4");
 
        _tokenService.HashToken("raw").Returns("hash");
        _refreshTokens.FirstOrDefaultAsync(Arg.Any<ISpecification<DomainRefreshToken>>(), Arg.Any<CancellationToken>())
            .Returns(expired);
 
        var handler = CreateHandler();
        var result = await handler.Handle(new RefreshTokenCommand("raw", "1.2.3.4"), CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.RefreshTokenExpired");
    }
 
    [Fact]
    public async Task Handle_Should_RotateToken_When_ValidAndActive()
    {
        var userId = Guid.NewGuid();
        var active = DomainRefreshToken.Create(userId, "hash", DateTime.UtcNow.AddDays(10), "1.2.3.4", "TestAgent");
        var user = new UserAuthInfo(userId, "user@example.com", new[] { "User" }, Array.Empty<string>());
 
        _tokenService.HashToken("raw").Returns("hash");
        _refreshTokens.FirstOrDefaultAsync(Arg.Any<ISpecification<DomainRefreshToken>>(), Arg.Any<CancellationToken>())
            .Returns(active);
        _identityService.GetUserByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(Result.Success(user));
        _tokenService.GenerateRefreshToken().Returns(("new-raw", "new-hash"));
        _tokenService.RefreshTokenLifetime.Returns(TimeSpan.FromDays(30));
        _tokenService.GenerateAccessToken(userId, user.Email, user.Roles, user.Permissions).Returns("new-access");
 
        var handler = CreateHandler();
        var result = await handler.Handle(new RefreshTokenCommand("raw", "1.2.3.4"), CancellationToken.None);
 
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("new-access");
        result.Value.RefreshToken.Should().Be("new-raw");
        active.IsRevoked.Should().BeTrue();
        await _refreshTokens.Received(1).AddAsync(Arg.Is<DomainRefreshToken>(rt => rt.DeviceName == "TestAgent"), Arg.Any<CancellationToken>());
    }
}