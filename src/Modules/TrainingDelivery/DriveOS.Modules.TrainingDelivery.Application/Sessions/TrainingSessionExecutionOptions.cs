namespace DriveOS.Modules.TrainingDelivery.Application.Sessions;

public sealed class TrainingSessionExecutionOptions
{
    public const string SectionName = "TrainingDelivery:SessionExecution";

    public int PreparationLeadMinutes { get; init; } = 30;
    public int StartEarlyToleranceMinutes { get; init; } = 15;
    public int StartLateToleranceMinutes { get; init; } = 180;
    public int ReadinessValidityMinutes { get; init; } = 5;
}
