using FluentAssertions;
using SocialHub.Application.Features.Diagnostics.Ping;
using Xunit;
 
namespace SocialHub.Application.Tests.Features.Diagnostics;
 
public class PingQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_ReturnSuccessfulPongResponse()
    {
        var handler = new PingQueryHandler();
 
        var result = await handler.Handle(new PingQuery(), CancellationToken.None);
 
        result.IsSuccess.Should().BeTrue();
        result.Value.Message.Should().Be("pong");
    }
}
 
public class FailPingQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_ReturnFailureResult()
    {
        var handler = new FailPingQueryHandler();
 
        var result = await handler.Handle(new FailPingQuery(), CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Ping.Forced");
    }
}