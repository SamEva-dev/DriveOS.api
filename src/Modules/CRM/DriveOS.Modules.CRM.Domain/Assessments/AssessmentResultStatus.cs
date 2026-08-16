namespace DriveOS.Modules.CRM.Domain.Assessments;

public enum AssessmentResultStatus
{
    None = 0,
    Draft = 1,
    CorrectionRequested = 2,
    Validated = 3,
    Shared = 4,
}

public enum AssessmentResultConfidence
{
    Low = 0,
    Medium = 1,
    High = 2,
}
