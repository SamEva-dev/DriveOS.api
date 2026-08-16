namespace DriveOS.Modules.Students.Domain.Transfers;

public enum InternalTransferMode
{
    Immediate = 1,
    EffectiveOnDate = 2,
    AfterCurrentPackage = 3,
    Temporary = 4,
}

[Flags]
public enum InternalTransferElement
{
    None = 0,
    Enrollment = 1,
    FutureSessions = 2,
    Instructor = 4,
    Vehicles = 8,
    Pricing = 16,
    Credits = 32,
    Documents = 64,
    Exams = 128,
    Payments = 256,
    Communications = 512,
    MeetingPoint = 1024,
    All = 2047,
}

public enum InternalTransferStatus
{
    Analyzed = 1,
    Scheduled = 2,
    Applied = 3,
    Cancelled = 4,
    Expired = 5,
    Reverted = 6,
}

public enum InternalTransferImpactType
{
    Enrollment = 1,
    FutureSessions = 2,
    Instructor = 3,
    Vehicles = 4,
    Pricing = 5,
    Credits = 6,
    Documents = 7,
    Exams = 8,
    Payments = 9,
    Communications = 10,
    MeetingPoint = 11,
}

public enum InternalTransferImpactStatus
{
    Passed = 1,
    Warning = 2,
    Blocked = 3,
    NotEvaluated = 4,
}
