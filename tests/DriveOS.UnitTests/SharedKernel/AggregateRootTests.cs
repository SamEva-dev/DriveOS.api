using DriveOS.SharedKernel.Domain;

namespace DriveOS.UnitTests.SharedKernel;

public sealed class AggregateRootTests
{
    [Fact]
    public void PullDomainEvents_ShouldReturnAndClearEvents()
    {
        var aggregate = new TestAggregate(Guid.NewGuid());

        aggregate.DoSomething();

        Assert.Single(aggregate.DomainEvents);

        var events = aggregate.PullDomainEvents();

        Assert.Single(events);
        Assert.Empty(aggregate.DomainEvents);
    }

    private sealed class TestAggregate : AggregateRoot<Guid>
    {
        public TestAggregate(Guid id)
            : base(id)
        {
        }

        public void DoSomething()
        {
            RaiseDomainEvent(new SomethingHappenedDomainEvent());
        }
    }

    private sealed record SomethingHappenedDomainEvent
        : DomainEvent;
}