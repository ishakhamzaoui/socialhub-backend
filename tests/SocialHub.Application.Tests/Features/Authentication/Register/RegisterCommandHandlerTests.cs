using FluentAssertions;
using NSubstitute;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Results;
using SocialHub.Application.Features.Authentication.Register;
using Xunit;
 
namespace SocialHub.Application.Tests.Features.Authentication.Register;
 
public class RegisterCommandHandlerTests
{
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly IEmailSender _emailSender = Substitute.For<IEmailSender>();
    private readonly IAppUrlProvider _appUrlProvider = Substitute.For<IAppUrlProvider>();
 
    private RegisterCommandHandler CreateHandler() => new(_identityService, _emailSender, _appUrlProvider);
 
    [Fact]
    public async Task Handle_Should_SendConfirmationEmail_When_UserCreatedSuccessfully()
    {
        var userId = Guid.NewGuid();
        _identityService.CreateUserAsync("user@example.com", "Str0ng!Pass1", Arg.Any<CancellationToken>())
            .Returns(Result.Success(userId));
        _identityService.GenerateEmailConfirmationTokenAsync(userId, Arg.Any<CancellationToken>())
            .Returns(Result.Success("confirm-token"));
        _appUrlProvider.BuildEmailConfirmationUrl(userId, "confirm-token").Returns("https://app.example.com/confirm");
 
        var handler = CreateHandler();
        var result = await handler.Handle(new RegisterCommand("user@example.com", "Str0ng!Pass1"), CancellationToken.None);
 
        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be(userId);
        await _emailSender.Received(1).SendAsync("user@example.com", Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
 
    [Fact]
    public async Task Handle_Should_ReturnFailure_When_EmailAlreadyRegistered()
    {
        var error = Error.Conflict("Auth.EmailAlreadyRegistered", "An account with this email already exists.");
        _identityService.CreateUserAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<Guid>(error));
 
        var handler = CreateHandler();
        var result = await handler.Handle(new RegisterCommand("dup@example.com", "Str0ng!Pass1"), CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.EmailAlreadyRegistered");
        await _emailSender.DidNotReceive().SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}