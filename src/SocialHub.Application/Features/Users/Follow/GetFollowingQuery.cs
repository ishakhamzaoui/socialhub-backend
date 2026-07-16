using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Users.Follow;
 
public sealed record GetFollowingQuery(Guid UserId, int Page = 1, int PageSize = 20) : IQuery<PagedUserListDto>, IRequireAuthorization;