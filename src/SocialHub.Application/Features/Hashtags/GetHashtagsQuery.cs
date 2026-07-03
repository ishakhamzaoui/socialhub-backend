using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Hashtags;
 
public sealed record GetHashtagsQuery : IQuery<IReadOnlyList<HashtagDto>>;