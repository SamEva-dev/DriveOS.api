namespace DriveOS.Modules.TrainingDelivery.Domain.Sessions;

public enum TrainingSessionInterventionType
{
    VerbalPrompt = 1,
    PhysicalControl = 2,
    DualControlUse = 3,
    EmergencyBraking = 4,
    SteeringIntervention = 5,
    RouteInstruction = 6,
    SessionPause = 7,
    SafetyStop = 8,
    Other = 99
}
