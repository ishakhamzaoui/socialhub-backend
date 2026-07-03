namespace SocialHub.Domain.Common;

/// <summary>
/// Marker for a domain event. Deliberately framework-independent: the Domain
/// layer must not reference MediatR or any other infrastructure library.
/// </summary>
public interface IDomainEvent
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
}
