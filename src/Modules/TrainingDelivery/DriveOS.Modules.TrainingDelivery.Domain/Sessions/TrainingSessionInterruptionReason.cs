namespace DriveOS.Modules.TrainingDelivery.Domain.Sessions;

public enum TrainingSessionInterruptionReason
{
    VehicleIssue = 1,
    StudentHealth = 2,
    InstructorHealth = 3,
    SafetyIncident = 4,
    Weather = 5,
    Administrative = 6,
    ExternalEvent = 7,
    TechnicalIssue = 8,
    Break = 9,
    Other = 99
}
