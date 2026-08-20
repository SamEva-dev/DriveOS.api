namespace DriveOS.Modules.SchedulingCapacity.Domain.Bookings;

public enum InstructorReplacementMode
{
    SingleSession = 1,
    SelectedSessions = 2,
    DateRange = 3,
    AllFutureSessions = 4,
    TemporaryAssignment = 5,
    PermanentReassignment = 6
}
