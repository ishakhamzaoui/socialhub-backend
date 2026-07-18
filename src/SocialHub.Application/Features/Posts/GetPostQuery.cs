using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Posts;
 
/// <summary>Roadmap 6.4. Visibility (including the block check) resolved via PostAccessPolicy before the post is ever returned.</summary>
public sealed record GetPostQuery(Guid PostId) : IQuery<PostDto>, IRequireAuthorization;