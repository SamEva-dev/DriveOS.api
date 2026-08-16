namespace DriveOS.Modules.Students.Domain.Enrollments;

public enum EnrollmentStatus
{
    Draft = 1,
    PendingDocuments = 2,
    ReadyForValidation = 3,
    Active = 4,
    Cancelled = 5,
    Suspended = 6,
    Closed = 7,
}
