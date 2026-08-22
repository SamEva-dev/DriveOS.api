namespace DriveOS.Modules.ExamsCertification.Domain.Registrations.Submissions;

public enum ExamRegistrationSubmissionStatus
{
    Pending = 1,
    Submitted = 2,
    Accepted = 3,
    Rejected = 4,
    CorrectionRequested = 5,
    Failed = 6,
    AwaitingManualSubmission = 7,
    Cancelled = 8
}
