namespace DriveOS.Modules.ExamsCertification.Domain.Registrations.Operations;

public enum ExamOperationalPlanStatus
{
    Draft = 1,
    ReadyForAssignment = 2,
    ConflictDetected = 3,
    Superseded = 4,
    Cancelled = 5
}
