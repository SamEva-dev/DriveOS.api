namespace DriveOS.Modules.Students.Domain.Branches;

[Flags]
public enum StudentBranchService
{
    None = 0,
    TheoryCourse = 1,
    PracticalLesson = 2,
    Simulator = 4,
    ExamSupport = 8,
    Administration = 16,
}

public enum StudentBranchAssignmentType
{
    Primary = 1,
    Secondary = 2,
    Temporary = 3,
}

public enum StudentBranchAssignmentStatus
{
    Planned = 1,
    Active = 2,
    Ended = 3,
    Cancelled = 4,
}

public enum BranchVerificationStatus
{
    Passed = 1,
    Failed = 2,
    NotEvaluated = 3,
    Warning = 4,
}

public enum BranchImpactType
{
    FutureSessions = 1,
    ReferenceInstructor = 2,
    LocalPricing = 3,
    MeetingPoint = 4,
    LocalDocumentsAndRules = 5,
}
