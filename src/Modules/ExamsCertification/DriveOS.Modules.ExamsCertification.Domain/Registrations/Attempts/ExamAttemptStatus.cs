namespace DriveOS.Modules.ExamsCertification.Domain.Registrations.Attempts;

/// <summary>Operational lifecycle of one concrete examination attempt.</summary>
public enum ExamAttemptStatus
{
    Scheduled = 1,
    CheckedIn = 2,
    EnRoute = 3,
    AtCenter = 4,
    InProgress = 5,
    AwaitingResult = 6,
    CandidateAbsent = 7,
    Postponed = 8,
    Cancelled = 9,
    Interrupted = 10,
    UnableToStart = 11
}

public enum ExamAttendanceStatus
{
    Expected = 1,
    Present = 2,
    Absent = 3,
    ExcusedAbsent = 4
}
