using FluentAssertions;
using NSubstitute;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Models;
using SocialHub.Application.Common.Results;
using SocialHub.Application.Features.Authentication.Login;
using SocialHub.Domain.Users;
using Xunit;
 
namespace SocialHub.Application.Tests.Features.Authentication.Login;
 
public class LoginCommandHandlerTests
{
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly IRepository<Domain.Users.RefreshToken, Guid> _refreshTokens = Substitute.For<IRepository<Domain.Users.RefreshToken, Guid>>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
 
    private LoginCommandHandler CreateHandler() => new(_identityService, _tokenService, _refreshTokens, _unitOfWork);
 
    [Fact]
    public async Task Handle_Should_IssueTokens_When_CredentialsAreValid()
    {
        var user = new UserAuthInfo(Guid.NewGuid(), "user@example.com", new[] { "User" }, Array.Empty<string>());
        _identityService.ValidateCredentialsAsync("user@example.com", "password", Arg.Any<CancellationToken>())
            .Returns(Result.Success(user));
        _tokenService.GenerateAccessToken(user.Id, user.Email, user.Roles, user.Permissions).Returns("access-token");
        _tokenService.GenerateRefreshToken().Returns(("raw-refresh", "hashed-refresh"));
        _tokenService.RefreshTokenLifetime.Returns(TimeSpan.FromDays(30));
 
        var handler = CreateHandler();
        var result = await handler.Handle(new LoginCommand("user@example.com", "password", "127.0.0.1", "TestAgent"), CancellationToken.None);
 
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access-token");
        result.Value.RefreshToken.Should().Be("raw-refresh");
        await _refreshTokens.Received(1).AddAsync(Arg.Is<Domain.Users.RefreshToken>(rt => rt.DeviceName == "TestAgent"), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
 
    [Fact]
    public async Task Handle_Should_ReturnFailure_When_CredentialsAreInvalid()
    {
        _identityService.ValidateCredentialsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<UserAuthInfo>(Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password.")));
 
        var handler = CreateHandler();
        var result = await handler.Handle(new LoginCommand("user@example.com", "wrong", null, null), CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.InvalidCredentials");
        await _refreshTokens.DidNotReceive().AddAsync(Arg.Any<Domain.Users.RefreshToken>(), Arg.Any<CancellationToken>());
    }
}