using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Results;
 
namespace SocialHub.Application.Features.Diagnostics.Ping;
 
public sealed class PingQueryHandler : IQueryHandler<PingQuery, PingResponse>
{
    public Task<Result<PingResponse>> Handle(PingQuery request, CancellationToken cancellationToken)
    {
        var response = new PingResponse("pong", DateTime.UtcNow);
        return Task.FromResult(Result.Success(response));
    }
}