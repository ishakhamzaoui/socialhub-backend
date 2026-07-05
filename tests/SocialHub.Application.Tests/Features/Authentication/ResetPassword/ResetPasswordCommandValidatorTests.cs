using FluentAssertions;
using SocialHub.Application.Features.Authentication.ResetPassword;
using Xunit;
 
namespace SocialHub.Application.Tests.Features.Authentication.ResetPassword;
 
public class ResetPasswordCommandValidatorTests
{
    private readonly ResetPasswordCommandValidator _validator = new();
 
    [Theory]
    [InlineData("short1!")]
    [InlineData("alllowercase1!")]
    [InlineData("ALLUPPERCASE1!")]
    [InlineData("NoDigitsHere!")]
    [InlineData("NoSpecial123")]
    public void Should_Fail_When_NewPasswordDoesNotMeetPolicy(string password)
    {
        var result = _validator.Validate(new ResetPasswordCommand("user@example.com", "some-token", password));
 
        result.IsValid.Should().BeFalse();
    }
 
    [Fact]
    public void Should_Fail_When_TokenIsEmpty()
    {
        var result = _validator.Validate(new ResetPasswordCommand("user@example.com", string.Empty, "Str0ng!Pass1"));
 
        result.IsValid.Should().BeFalse();
    }
 
    [Fact]
    public void Should_Pass_When_AllFieldsAreValid()
    {
        var result = _validator.Validate(new ResetPasswordCommand("user@example.com", "some-token", "Str0ng!Pass1"));
 
        result.IsValid.Should().BeTrue();
    }
}