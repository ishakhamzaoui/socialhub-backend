using MediatR;
using SocialHub.Domain.Common;

namespace SocialHub.Application.Common.Events;

public sealed class DomainEventNotification<TDomainEvent> : INotification
    where TDomainEvent : IDomainEvent
{
    public DomainEventNotification(TDomainEvent domainEvent)
    {
        DomainEvent = domainEvent;
    }
 
    public TDomainEvent DomainEvent { get; }
}
