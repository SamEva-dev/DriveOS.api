namespace DriveOS.Modules.SchedulingCapacity.Domain.Bookings;

public enum AttendanceStatus
{
    Present = 1,
    LateArrival = 2,
    StudentAbsent = 3,
    InstructorAbsent = 4,
    PartialAttendance = 5,
    ExcusedAbsence = 6,
    UnexcusedAbsence = 7,
    CancelledBeforeStart = 8,
    UnableToDeliver = 9
}

public enum AttendanceChargeDecision
{
    None = 0,
    NoCharge = 1,
    PendingExternalReview = 2,
    ChargeRequested = 3,
    PartialChargeRequested = 4
}

public enum AttendanceCreditDecision
{
    None = 0,
    CreditPreserved = 1,
    PendingExternalReview = 2,
    ConsumptionRequested = 3,
    PartialConsumptionRequested = 4
}

public enum AttendanceFollowUpAction
{
    None = 0,
    RequestEvidence = 1,
    Reschedule = 2,
    CreateIncident = 3,
    ContactStudent = 4,
    ReplaceInstructor = 5,
    ManualReview = 6
}
