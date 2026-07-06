using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Media;
 
public sealed record DeleteMediaCommand(Guid MediaId) : ICommand, IRequireAuthorization;