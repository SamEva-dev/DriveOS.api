namespace DriveOS.Modules.Students.Domain.Guardians;

public enum GuardianRelationshipType
{
    Parent = 1,
    LegalGuardian = 2,
    FosterParent = 3,
    AuthorizedRepresentative = 4,
    Other = 5,
}

public enum ParentalAuthorityStatus
{
    Unknown = 0,
    Full = 1,
    Shared = 2,
    Restricted = 3,
    None = 4,
}

public enum GuardianRelationshipStatus
{
    Active = 1,
    Suspended = 2,
    Revoked = 3,
    Expired = 4,
}

[Flags]
public enum GuardianPermissions : long
{
    None = 0,
    ProfileRead = 1 << 0,
    ScheduleRead = 1 << 1,
    ScheduleBook = 1 << 2,
    ScheduleCancel = 1 << 3,
    ProgressRead = 1 << 4,
    DocumentsRead = 1 << 5,
    DocumentsUpload = 1 << 6,
    ContractsSign = 1 << 7,
    InvoicesRead = 1 << 8,
    PaymentsPay = 1 << 9,
    ExamRead = 1 << 10,
    MessagesRead = 1 << 11,
    MessagesSend = 1 << 12,
}
