using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Features.Users.Follow;
 
namespace SocialHub.Application.Features.Users.Safety;
 
/// <summary>Not paginated — same scope simplification as GetBlockedUsersQuery.</summary>
public sealed record GetMutedUsersQuery : IQuery<IReadOnlyList<UserSummaryDto>>, IRequireAuthorization;