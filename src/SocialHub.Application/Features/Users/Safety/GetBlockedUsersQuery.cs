using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Features.Users.Follow;
 
namespace SocialHub.Application.Features.Users.Safety;
 
/// <summary>Not paginated (deliberate scope simplification — block lists are small in practice; add pagination here if that stops being true).</summary>
public sealed record GetBlockedUsersQuery : IQuery<IReadOnlyList<UserSummaryDto>>, IRequireAuthorization;