namespace DriveOS.Modules.ExamsCertification.Domain.Registrations.Attempts;

/// <summary>Purpose-limited point-in-time location capture. Continuous tracking is intentionally out of scope.</summary>
public enum ExamAttemptLocationPurpose
{
    Assistance = 1,
    Route = 2,
    ArrivalConfirmation = 3,
    Coordination = 4,
    Incident = 5,
    Return = 6
}
