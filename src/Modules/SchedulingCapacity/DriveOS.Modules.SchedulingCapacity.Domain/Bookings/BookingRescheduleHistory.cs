using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Bookings;

public sealed class BookingRescheduleHistory
{
    private BookingRescheduleHistory() { }

    internal BookingRescheduleHistory(
        BookingRescheduleId id,
        BookingId bookingId,
        Guid operationId,
        DateTimeOffset previousStartAtUtc,
        DateTimeOffset previousEndAtUtc,
        DateTimeOffset newStartAtUtc,
        DateTimeOffset newEndAtUtc,
        BranchId? previousBranchId,
        BranchId? newBranchId,
        BookingStatus previousStatus,
        string reason,
        bool resourcesChanged,
        string previousResourceFingerprint,
        string newResourceFingerprint,
        DateTimeOffset occurredAtUtc)
    {
        Id = id;
        BookingId = bookingId;
        OperationId = operationId;
        PreviousStartAtUtc = previousStartAtUtc.ToUniversalTime();
        PreviousEndAtUtc = previousEndAtUtc.ToUniversalTime();
        NewStartAtUtc = newStartAtUtc.ToUniversalTime();
        NewEndAtUtc = newEndAtUtc.ToUniversalTime();
        PreviousBranchId = previousBranchId;
        NewBranchId = newBranchId;
        PreviousStatus = previousStatus;
        Reason = reason;
        ResourcesChanged = resourcesChanged;
        PreviousResourceFingerprint = previousResourceFingerprint;
        NewResourceFingerprint = newResourceFingerprint;
        OccurredAtUtc = occurredAtUtc.ToUniversalTime();
    }

    public BookingRescheduleId Id { get; private set; }
    public BookingId BookingId { get; private set; }
    public Guid OperationId { get; private set; }
    public DateTimeOffset PreviousStartAtUtc { get; private set; }
    public DateTimeOffset PreviousEndAtUtc { get; private set; }
    public DateTimeOffset NewStartAtUtc { get; private set; }
    public DateTimeOffset NewEndAtUtc { get; private set; }
    public BranchId? PreviousBranchId { get; private set; }
    public BranchId? NewBranchId { get; private set; }
    public BookingStatus PreviousStatus { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public bool ResourcesChanged { get; private set; }
    public string PreviousResourceFingerprint { get; private set; } = string.Empty;
    public string NewResourceFingerprint { get; private set; } = string.Empty;
    public DateTimeOffset OccurredAtUtc { get; private set; }
}
