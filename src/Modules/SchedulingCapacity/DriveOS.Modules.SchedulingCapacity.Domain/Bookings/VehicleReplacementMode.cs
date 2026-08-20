namespace DriveOS.Modules.SchedulingCapacity.Domain.Bookings;

public enum VehicleReplacementMode
{
    SingleSession = 1,
    SelectedSessions = 2,
    DateRange = 3,
    UntilRepairCompleted = 4,
    PermanentReplacement = 5
}
