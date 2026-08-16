namespace DriveOS.SharedKernel.Domain;

public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();

    public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
}
