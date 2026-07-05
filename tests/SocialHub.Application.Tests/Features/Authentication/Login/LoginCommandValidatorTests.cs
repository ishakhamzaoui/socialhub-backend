using FluentAssertions;
using SocialHub.Application.Features.Authentication.Login;
using Xunit;
 
namespace SocialHub.Application.Tests.Features.Authentication.Login;
 
public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();
 
    [Fact]
    public void Should_Fail_When_EmailIsEmpty()
    {
        var result = _validator.Validate(new LoginCommand(string.Empty, "password", null, null));
 
        result.IsValid.Should().BeFalse();
    }
 
    [Fact]
    public void Should_Fail_When_PasswordIsEmpty()
    {
        var result = _validator.Validate(new LoginCommand("user@example.com", string.Empty, null, null));
 
        result.IsValid.Should().BeFalse();
    }
 
    [Fact]
    public void Should_Pass_When_EmailAndPasswordAreProvided()
    {
        var result = _validator.Validate(new LoginCommand("user@example.com", "password", "1.2.3.4", "TestAgent"));
 
        result.IsValid.Should().BeTrue();
    }
}