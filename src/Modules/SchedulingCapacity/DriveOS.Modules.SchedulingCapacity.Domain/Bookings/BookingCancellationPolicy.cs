namespace DriveOS.Modules.SchedulingCapacity.Domain.Bookings;

public enum CancellationInitiator
{
    Student = 1,
    Guardian = 2,
    Instructor = 3,
    Organization = 4,
    Partner = 5,
    System = 6,
    ForceMajeure = 7
}

public enum CancellationReasonCode
{
    Illness = 1,
    Unavailability = 2,
    Breakdown = 3,
    Weather = 4,
    SchedulingError = 5,
    InstructorAbsent = 6,
    StudentRequest = 7,
    Administrative = 8,
    ForceMajeure = 9,
    Other = 10
}

public enum BookingCreditDecision
{
    PendingExternalReview = 1,
    Released = 2,
    Consumed = 3,
    PartiallyConsumed = 4,
    NotApplicable = 5
}

public enum BookingFeeDecision
{
    PendingExternalReview = 1,
    NoCharge = 2,
    PartialCharge = 3,
    FeeApplied = 4,
    NotApplicable = 5
}

public enum BookingNotificationDecision
{
    NotifyAffectedParticipants = 1,
    NotifyInitiatorOnly = 2,
    None = 3
}
