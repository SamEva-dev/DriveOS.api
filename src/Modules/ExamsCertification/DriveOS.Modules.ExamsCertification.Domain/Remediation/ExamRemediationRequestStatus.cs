namespace DriveOS.Modules.ExamsCertification.Domain.Remediation;

public enum ExamRemediationRequestStatus
{
    PendingConfiguration = 0,
    ReadyToProvision = 1,
    Provisioning = 2,
    Planned = 3,
    InProgress = 4,
    Completed = 5,
    ValidatedForRePresentation = 6,
    Deferred = 7,
    Failed = 8,
    Cancelled = 9,
    Superseded = 10
}
