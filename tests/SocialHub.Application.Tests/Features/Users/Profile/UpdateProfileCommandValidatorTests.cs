using FluentAssertions;
using SocialHub.Application.Features.Users.Profile;
using Xunit;
 
namespace SocialHub.Application.Tests.Features.Users.Profile;
 
public class UpdateProfileCommandValidatorTests
{
    private readonly UpdateProfileCommandValidator _validator = new();
 
    [Fact]
    public void Should_Pass_ForAValidCommand()
    {
        var result = _validator.Validate(new UpdateProfileCommand("Alice", "bio", "NYC", "https://example.com"));
 
        result.IsValid.Should().BeTrue();
    }
 
    [Fact]
    public void Should_Fail_When_DisplayNameIsEmpty()
    {
        var result = _validator.Validate(new UpdateProfileCommand(string.Empty, null, null, null));
 
        result.IsValid.Should().BeFalse();
    }
 
    [Fact]
    public void Should_Pass_When_WebsiteIsNull()
    {
        var result = _validator.Validate(new UpdateProfileCommand("Alice", null, null, null));
 
        result.IsValid.Should().BeTrue();
    }
 
    [Fact]
    public void Should_Fail_When_WebsiteIsNotAnAbsoluteUrl()
    {
        var result = _validator.Validate(new UpdateProfileCommand("Alice", null, null, "not-a-url"));
 
        result.IsValid.Should().BeFalse();
    }
}