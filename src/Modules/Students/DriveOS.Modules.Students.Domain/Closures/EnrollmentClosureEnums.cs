namespace DriveOS.Modules.Students.Domain.Closures;

public enum EnrollmentClosureReason
{
    TrainingCompleted = 1,
    LicenseObtained = 2,
    Transferred = 3,
    StudentWithdrawal = 4,
    ContractTerminated = 5,
    Exclusion = 6,
    OrganizationClosure = 7,
    DuplicateResolved = 8,
    Other = 9,
}

public enum EnrollmentClosureStatus
{
    Draft = 1,
    ReadyToClose = 2,
    Closed = 3,
    Archived = 4,
    Reopened = 5,
    Cancelled = 6,
}

public enum EnrollmentClosureCheckType
{
    FutureSessions = 1,
    FinalInvoices = 2,
    Credits = 3,
    Exams = 4,
    Documents = 5,
    Contract = 6,
    Equipment = 7,
    Disputes = 8,
    DataRetention = 9,
}

public enum EnrollmentClosureCheckStatus
{
    Pending = 1,
    Resolved = 2,
    NotApplicable = 3,
    Blocking = 4,
}

[Flags]
public enum StudentDataRetentionScope
{
    None = 0,
    Identity = 1,
    Contracts = 2,
    Finance = 4,
    Pedagogy = 8,
    Exams = 16,
    Documents = 32,
    Audit = 64,
    Disputes = 128,
}

public sealed record EnrollmentClosureCheckSeed(
    EnrollmentClosureCheckType Type,
    EnrollmentClosureCheckStatus Status,
    string Detail
);
