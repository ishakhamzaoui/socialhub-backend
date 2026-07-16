using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Users.Follow;
 
/// <summary>Roadmap 5.14. Always about the current user (no target userId) — mutual-followers-based only, per the Phase 5 scope decision. See IFollowRepository.GetSuggestedUserIdsAsync.</summary>
public sealed record GetSuggestedUsersQuery(int Limit = 20) : IQuery<IReadOnlyList<UserSummaryDto>>, IRequireAuthorization;