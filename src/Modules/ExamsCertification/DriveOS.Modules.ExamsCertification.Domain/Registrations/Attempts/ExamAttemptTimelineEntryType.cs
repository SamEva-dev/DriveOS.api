namespace DriveOS.Modules.ExamsCertification.Domain.Registrations.Attempts;

public enum ExamAttemptTimelineEntryType
{
    AttemptCreated = 1,
    CheckedIn = 2,
    DepartureRecorded = 3,
    ArrivalRecorded = 4,
    ExamStarted = 5,
    ExamCompleted = 6,
    ReturnRecorded = 7,
    IncidentReported = 8,
    OperationalNoteAdded = 9,
    LocationRecorded = 10,
    ResourceChangeRecorded = 11,
    CandidateAbsent = 12,
    Postponed = 13,
    Cancelled = 14,
    Interrupted = 15,
    UnableToStart = 16
}
