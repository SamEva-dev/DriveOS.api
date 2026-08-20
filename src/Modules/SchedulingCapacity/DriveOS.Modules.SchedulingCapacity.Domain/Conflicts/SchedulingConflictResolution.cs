namespace DriveOS.Modules.SchedulingCapacity.Domain.Conflicts;

public enum SchedulingConflictResolution
{
    Reschedule = 1,
    ReassignInstructor = 2,
    ReassignVehicle = 3,
    ChangeLocation = 4,
    AdjustMargin = 5,
    CancelBooking = 6,
    AcceptRiskWithReason = 7,
    RequestDecision = 8,
    Other = 99
}
