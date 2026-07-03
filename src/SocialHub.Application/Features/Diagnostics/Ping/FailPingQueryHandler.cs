using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Results;
 
namespace SocialHub.Application.Features.Diagnostics.Ping;
 
public sealed class FailPingQueryHandler : IQueryHandler<FailPingQuery, PingResponse>
{
    public Task<Result<PingResponse>> Handle(FailPingQuery request, CancellationToken cancellationToken)
    {
        var error = Error.Failure("Ping.Forced", "This failure was forced to exercise the exception/result pipeline.");
        return Task.FromResult(Result.Failure<PingResponse>(error));
    }
}