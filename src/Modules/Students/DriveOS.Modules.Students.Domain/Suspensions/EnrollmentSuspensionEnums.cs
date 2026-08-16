namespace DriveOS.Modules.Students.Domain.Suspensions;

public enum EnrollmentSuspensionReason
{
    StudentRequest = 1,
    MedicalOrPersonalReason = 2,
    AdministrativeIssue = 3,
    FinancialIssue = 4,
    DisciplinaryMeasure = 5,
    LongAbsence = 6,
    FundingPause = 7,
    ComplianceIssue = 8,
    Other = 9,
}

[Flags]
public enum EnrollmentSuspensionScope
{
    None = 0,
    FullEnrollment = 1,
    SchedulingOnly = 2,
    TrainingDelivery = 4,
    ExamRegistration = 8,
    PortalAccess = 16,
    FinanceActions = 32,
    All = 63,
}

public enum ExistingBookingsDecision
{
    Keep = 1,
    CancelWithoutCharge = 2,
    CancelWithPolicy = 3,
    Reschedule = 4,
    ManualReview = 5,
}

public enum EnrollmentSuspensionStatus
{
    Scheduled = 1,
    Active = 2,
    Ended = 3,
    Cancelled = 4,
}

public enum SuspensionNotificationStatus
{
    Queued = 1,
    Sent = 2,
    Failed = 3,
}

public enum EnrollmentReactivationMode
{
    Immediate = 1,
    Scheduled = 2,
    Conditional = 3,
    NewEnrollment = 4,
}

public enum ReactivationCheckType
{
    SuspensionReasonResolved = 1,
    Contract = 2,
    Documents = 3,
    Funding = 4,
    Credits = 5,
    Pedagogy = 6,
    Instructor = 7,
    Resources = 8,
    Assessment = 9,
    Planning = 10,
    RegulatoryRules = 11,
}

public enum ReactivationCheckStatus
{
    Valid = 1,
    Warning = 2,
    Failed = 3,
    NotApplicable = 4,
}

public enum EnrollmentReactivationStatus
{
    PendingConditions = 1,
    Scheduled = 2,
    Applied = 3,
    NewEnrollmentRequired = 4,
    Cancelled = 5,
}
