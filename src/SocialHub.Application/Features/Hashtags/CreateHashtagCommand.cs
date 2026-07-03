using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Hashtags;
 
public sealed record CreateHashtagCommand(string Tag) : ICommand<HashtagDto>, ITransactionalRequest;