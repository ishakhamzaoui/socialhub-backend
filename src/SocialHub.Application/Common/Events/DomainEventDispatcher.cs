using MediatR;
using SocialHub.Domain.Common;
 
namespace SocialHub.Application.Common.Events;
 
public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IPublisher _publisher;
 
    public DomainEventDispatcher(IPublisher publisher)
    {
        _publisher = publisher;
    }
 
    public async Task DispatchAndClearEvents(IEnumerable<BaseEntity> entitiesWithEvents, CancellationToken cancellationToken = default)
    {
        foreach (var entity in entitiesWithEvents)
        {
            var events = entity.DomainEvents.ToArray();
            entity.ClearDomainEvents();
 
            foreach (var domainEvent in events)
            {
                var wrapperType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
                var notification = (INotification)Activator.CreateInstance(wrapperType, domainEvent)!;
                await _publisher.Publish(notification, cancellationToken);
            }
        }
    }
}