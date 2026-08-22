namespace DriveOS.Modules.ExamsCertification.Domain.Results.Success;

public enum ExamSuccessActionStatus
{
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Deferred = 4,
    Failed = 5,
    Blocked = 6,
    NotApplicable = 7,
    Superseded = 8
}
