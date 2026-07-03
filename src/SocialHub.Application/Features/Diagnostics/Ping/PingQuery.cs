using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Diagnostics.Ping;
 
public sealed record PingQuery : IQuery<PingResponse>;
 
public sealed record PingResponse(string Message, DateTime ServerTimeUtc);