using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Availability;

public sealed class AvailabilityException
{
    private AvailabilityException() { }

    internal AvailabilityException(
        AvailabilityExceptionId id,
        AvailabilityPlanId availabilityPlanId,
        DateOnly date,
        TimeOnly startTime,
        TimeOnly endTime,
        AvailabilityExceptionType type,
        AvailabilityExceptionSource source,
        int priority,
        int? capacity,
        string? reason)
    {
        Id = id;
        AvailabilityPlanId = availabilityPlanId;
        Date = date;
        StartTime = startTime;
        EndTime = endTime;
        Type = type;
        Source = source;
        Priority = priority;
        Capacity = capacity;
        Reason = reason;
    }

    public AvailabilityExceptionId Id { get; private set; }
    public AvailabilityPlanId AvailabilityPlanId { get; private set; }
    public DateOnly Date { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public AvailabilityExceptionType Type { get; private set; }
    public AvailabilityExceptionSource Source { get; private set; }
    public int Priority { get; private set; }
    public int? Capacity { get; private set; }
    public string? Reason { get; private set; }

    internal bool Covers(DateOnly date, TimeOnly startTime, TimeOnly endTime) =>
        Date == date && startTime >= StartTime && endTime <= EndTime;

    internal bool Overlaps(DateOnly date, TimeOnly startTime, TimeOnly endTime) =>
        Date == date && startTime < EndTime && endTime > StartTime;
}
