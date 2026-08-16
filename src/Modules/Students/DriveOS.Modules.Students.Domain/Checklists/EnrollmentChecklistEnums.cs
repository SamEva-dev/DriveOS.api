namespace DriveOS.Modules.Students.Domain.Checklists;

public enum ChecklistCategory
{
    Identity = 1,
    Documents = 2,
    Contract = 3,
    Finance = 4,
    Pedagogy = 5,
    UserAccount = 6,
    Guardians = 7,
    SchedulingAuthorization = 8,
}

public enum ChecklistItemStatus
{
    NotStarted = 1,
    InProgress = 2,
    WaitingExternal = 3,
    Completed = 4,
    Waived = 5,
    Rejected = 6,
    Blocked = 7,
    Expired = 8,
}
