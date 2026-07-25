namespace DriveOS.SharedKernel.Domain;

public interface IDomainEvent
{
    Guid EventId { get; }

    DateTimeOffset OccurredAtUtc { get; }
}