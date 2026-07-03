using FluentAssertions;
using SocialHub.Application.Common.Results;
using Xunit;
 
namespace SocialHub.Application.Tests.Common;
 
public class ResultTests
{
    [Fact]
    public void Success_Should_BeSuccessWithNoError()
    {
        var result = Result.Success();
 
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }
 
    [Fact]
    public void Failure_Should_BeFailureAndCarryError()
    {
        var error = Error.Failure("Test.Code", "Something went wrong.");
 
        var result = Result.Failure(error);
 
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(error);
    }
 
    [Fact]
    public void GenericSuccess_Should_ExposeValue()
    {
        var result = Result.Success(42);
 
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }
 
    [Fact]
    public void GenericFailure_Should_ThrowWhenAccessingValue()
    {
        var result = Result.Failure<int>(Error.NotFound("Test.NotFound", "Not found."));
 
        var act = () => result.Value;
 
        act.Should().Throw<InvalidOperationException>();
    }
 
    [Fact]
    public void ImplicitConversion_Should_ProduceSuccessResult()
    {
        Result<string> result = "hello";
 
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("hello");
    }
 
    [Theory]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Unauthorized)]
    [InlineData(ErrorType.Forbidden)]
    [InlineData(ErrorType.Failure)]
    public void Failure_Should_PreserveErrorType(ErrorType errorType)
    {
        var error = new Error("Test.Code", "message", errorType);
 
        var result = Result.Failure(error);
 
        result.Error.Type.Should().Be(errorType);
    }
}