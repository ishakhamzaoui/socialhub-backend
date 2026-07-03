using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Diagnostics.Ping;
 
/// <summary>
/// Deliberately-failing query, used to exercise the Result/ProblemDetails
/// path end-to-end (see the roadmap's Phase 1 exit criteria).
/// </summary>
public sealed record FailPingQuery : IQuery<PingResponse>;