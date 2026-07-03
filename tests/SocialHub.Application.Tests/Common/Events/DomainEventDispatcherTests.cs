using FluentAssertions;
using MediatR;
using NSubstitute;
using SocialHub.Application.Common.Events;
using SocialHub.Domain.Common;
using Xunit;
 
namespace SocialHub.Application.Tests.Common.Events;
 
public class DomainEventDispatcherTests
{
    private sealed record TestDomainEvent : BaseEvent;
 
    private sealed class TestEntity : BaseEntity
    {
        public void RaiseEvent() => AddDomainEvent(new TestDomainEvent());
    }
 
    [Fact]
    public async Task DispatchAndClearEvents_Should_PublishEachQueuedEvent()
    {
        var publisher = Substitute.For<IPublisher>();
        var dispatcher = new DomainEventDispatcher(publisher);
 
        var entity = new TestEntity();
        entity.RaiseEvent();
        entity.RaiseEvent();
 
        await dispatcher.DispatchAndClearEvents(new[] { entity });
 
        await publisher.Received(2).Publish(Arg.Any<DomainEventNotification<TestDomainEvent>>(), Arg.Any<CancellationToken>());
    }
 
    [Fact]
    public async Task DispatchAndClearEvents_Should_ClearEventsAfterDispatch()
    {
        var publisher = Substitute.For<IPublisher>();
        var dispatcher = new DomainEventDispatcher(publisher);
 
        var entity = new TestEntity();
        entity.RaiseEvent();
 
        await dispatcher.DispatchAndClearEvents(new[] { entity });
 
        entity.DomainEvents.Should().BeEmpty();
    }
 
    [Fact]
    public async Task DispatchAndClearEvents_Should_DoNothingForEntityWithNoEvents()
    {
        var publisher = Substitute.For<IPublisher>();
        var dispatcher = new DomainEventDispatcher(publisher);
 
        var entity = new TestEntity();
 
        await dispatcher.DispatchAndClearEvents(new[] { entity });
 
        await publisher.DidNotReceive().Publish(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }
}