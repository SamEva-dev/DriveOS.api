namespace DriveOS.Modules.ExamsCertification.Domain.Registrations;

public enum ExamRegistrationStatus
{
    Draft = 1,
    PlaceAssigned = 2,
    PendingSubmission = 3,
    Submitted = 4,
    Confirmed = 5,
    Rejected = 6,
    Cancelled = 7,
    Completed = 8,
    CorrectionRequested = 9
}
