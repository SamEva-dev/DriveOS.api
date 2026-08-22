namespace DriveOS.Modules.ExamsCertification.Domain.Registrations.Preparation;

public enum ExamPreparationStatus
{
    Incomplete = 1,
    Ready = 2,
    Blocked = 3
}

public enum ExamPreparationCheckStatus
{
    Pending = 1,
    Ready = 2,
    Warning = 3,
    Blocked = 4,
    NotApplicable = 5
}
