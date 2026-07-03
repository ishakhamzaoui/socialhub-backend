using FluentAssertions;
using SocialHub.Domain.Common;
using Xunit;
 
namespace SocialHub.Domain.Tests.Common;
 
public class BaseEntityTests
{
    private sealed record TestEvent : BaseEvent;
 
    private sealed class TestEntity : BaseEntity
    {
        public void RaiseTestEvent() => AddDomainEvent(new TestEvent());
    }
 
    [Fact]
    public void AddDomainEvent_Should_AddEventToCollection()
    {
        var entity = new TestEntity();
 
        entity.RaiseTestEvent();
 
        entity.DomainEvents.Should().HaveCount(1);
        entity.DomainEvents.Single().Should().BeOfType<TestEvent>();
    }
 
    [Fact]
    public void ClearDomainEvents_Should_EmptyCollection()
    {
        var entity = new TestEntity();
        entity.RaiseTestEvent();
 
        entity.ClearDomainEvents();
 
        entity.DomainEvents.Should().BeEmpty();
    }
 
    [Fact]
    public void NewEntity_Should_HaveNoEventsByDefault()
    {
        var entity = new TestEntity();
 
        entity.DomainEvents.Should().BeEmpty();
    }
}