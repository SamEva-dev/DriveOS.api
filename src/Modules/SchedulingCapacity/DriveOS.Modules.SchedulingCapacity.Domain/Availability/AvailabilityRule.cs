using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Availability;

public sealed class AvailabilityRule
{
    private AvailabilityRule() { }

    internal AvailabilityRule(
        AvailabilityRuleId id,
        AvailabilityPlanId availabilityPlanId,
        DayOfWeek dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime,
        int capacity,
        AvailabilityRuleType type,
        AvailabilityExceptionSource source,
        int priority,
        BranchId? branchId,
        string? trainingCategory,
        string? serviceArea)
    {
        Id = id;
        AvailabilityPlanId = availabilityPlanId;
        DayOfWeek = dayOfWeek;
        StartTime = startTime;
        EndTime = endTime;
        Capacity = capacity;
        Type = type;
        Source = source;
        Priority = priority;
        BranchId = branchId;
        TrainingCategory = trainingCategory;
        ServiceArea = serviceArea;
    }

    public AvailabilityRuleId Id { get; private set; }
    public AvailabilityPlanId AvailabilityPlanId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public int Capacity { get; private set; }
    public AvailabilityRuleType Type { get; private set; }
    public AvailabilityExceptionSource Source { get; private set; }
    public int Priority { get; private set; }
    public BranchId? BranchId { get; private set; }
    public string? TrainingCategory { get; private set; }
    public string? ServiceArea { get; private set; }

    internal bool Covers(DayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime) =>
        DayOfWeek == dayOfWeek && startTime >= StartTime && endTime <= EndTime;

    internal bool Overlaps(DayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime) =>
        DayOfWeek == dayOfWeek && startTime < EndTime && endTime > StartTime;
}
