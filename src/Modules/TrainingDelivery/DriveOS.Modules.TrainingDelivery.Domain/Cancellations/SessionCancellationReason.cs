namespace DriveOS.Modules.TrainingDelivery.Domain.Cancellations;

public enum SessionCancellationReason
{
    Safety = 1,
    VehicleIssue = 2,
    StudentHealth = 3,
    InstructorHealth = 4,
    StudentRequest = 5,
    InstructorRequest = 6,
    OrganizationDecision = 7,
    Weather = 8,
    ExternalEvent = 9,
    Incident = 10,
    Other = 11
}
