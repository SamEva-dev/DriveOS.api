namespace DriveOS.Modules.Students.Domain.Statuses;

public enum FinancialStatus
{
    Unknown = 0,
    UpToDate = 1,
    Pending = 2,
    Overdue = 3,
    Blocked = 4,
}

public enum PedagogicalStatus
{
    NotStarted = 1,
    InProgress = 2,
    ReadyForExam = 3,
    Completed = 4,
    Blocked = 5,
}

public enum SchedulingStatus
{
    Allowed = 1,
    Restricted = 2,
    Suspended = 3,
}

public enum ExamStatus
{
    NotReady = 1,
    Ready = 2,
    Registered = 3,
    Passed = 4,
    Failed = 5,
}

public enum PortalAccessStatus
{
    NotInvited = 1,
    Invited = 2,
    Active = 3,
    Suspended = 4,
    Revoked = 5,
}

[Flags]
public enum StudentBlockingAction
{
    None = 0,
    Schedule = 1,
    StartLesson = 2,
    Sign = 4,
    PresentExam = 8,
    Transfer = 16,
    Refund = 32,
    Close = 64,
    PortalAccess = 128,
}

public enum StudentBlockSeverity
{
    Information = 1,
    Warning = 2,
    Blocking = 3,
    Critical = 4,
}

public enum StudentBlockStatus
{
    Active = 1,
    Overridden = 2,
    Released = 3,
    Resolved = 4,
}

public enum StudentBlockResolutionType
{
    HumanValidation = 1,
    AutomaticEvent = 2,
    TemporaryOverride = 3,
    Payment = 4,
    Document = 5,
    PedagogicalDecision = 6,
}
