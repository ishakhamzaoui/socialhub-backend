using SocialHub.Domain.Common;
 
namespace SocialHub.Application.Common.Events;
 
public interface IDomainEventDispatcher
{
    Task DispatchAndClearEvents(IEnumerable<BaseEntity> entitiesWithEvents, CancellationToken cancellationToken = default);
}