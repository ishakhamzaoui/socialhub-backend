using FluentAssertions;
using SocialHub.Application.Features.Authentication.Register;
using Xunit;
 
namespace SocialHub.Application.Tests.Features.Authentication.Register;
 
public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();
 
    [Fact]
    public void Should_Fail_When_EmailIsInvalid()
    {
        var result = _validator.Validate(new RegisterCommand("not-an-email", "Str0ng!Pass1"));
 
        result.IsValid.Should().BeFalse();
    }
 
    [Theory]
    [InlineData("short1!")]        // too short
    [InlineData("alllowercase1!")] // no uppercase
    [InlineData("ALLUPPERCASE1!")] // no lowercase
    [InlineData("NoDigitsHere!")]  // no digit
    [InlineData("NoSpecial123")]   // no special char
    public void Should_Fail_When_PasswordDoesNotMeetPolicy(string password)
    {
        var result = _validator.Validate(new RegisterCommand("user@example.com", password));
 
        result.IsValid.Should().BeFalse();
    }
 
    [Fact]
    public void Should_Pass_When_EmailAndPasswordAreValid()
    {
        var result = _validator.Validate(new RegisterCommand("user@example.com", "Str0ng!Pass1"));
 
        result.IsValid.Should().BeTrue();
    }
}