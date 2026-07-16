using FluentAssertions;
using SocialHub.Application.Features.Users.Profile;
using Xunit;
 
namespace SocialHub.Application.Tests.Features.Users.Profile;
 
public class UpdateLanguagePreferenceCommandValidatorTests
{
    private readonly UpdateLanguagePreferenceCommandValidator _validator = new();
 
    [Theory]
    [InlineData("en")]
    [InlineData("FR")]
    public void Should_Pass_ForATwoLetterCode(string language)
    {
        var result = _validator.Validate(new UpdateLanguagePreferenceCommand(language));
 
        result.IsValid.Should().BeTrue();
    }
 
    [Theory]
    [InlineData("")]
    [InlineData("english")]
    [InlineData("e1")]
    public void Should_Fail_ForAnInvalidCode(string language)
    {
        var result = _validator.Validate(new UpdateLanguagePreferenceCommand(language));
 
        result.IsValid.Should().BeFalse();
    }
}