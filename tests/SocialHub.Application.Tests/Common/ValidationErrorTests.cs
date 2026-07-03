using FluentAssertions;
using SocialHub.Application.Common.Results;
using Xunit;
 
namespace SocialHub.Application.Tests.Common;
 
public class ValidationErrorTests
{
    [Fact]
    public void ValidationError_Should_ExposeUnderlyingErrors()
    {
        var errors = new List<Error>
        {
            Error.Validation("Name", "Name is required."),
            Error.Validation("Email", "Email is invalid.")
        };
 
        var validationError = new ValidationError(errors);
 
        validationError.Type.Should().Be(ErrorType.Validation);
        validationError.Errors.Should().HaveCount(2);
        validationError.Errors.Should().BeEquivalentTo(errors);
    }
}