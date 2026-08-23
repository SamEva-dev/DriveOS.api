namespace DriveOS.Modules.RegulatoryIntegrations.Infrastructure.Dispatching;

public sealed class RegulatoryTrainingRecordDispatchOptions
{
    public const string SectionName = "RegulatoryIntegrations:TrainingRecordDispatch";

    public bool Enabled { get; init; } = true;
    public int PollSeconds { get; init; } = 5;
    public int BatchSize { get; init; } = 50;
    public int ProcessingLeaseMinutes { get; init; } = 5;
    public int DefaultRetryMinutes { get; init; } = 5;
    public int UnavailableRetryMinutes { get; init; } = 360;
    public int MaxRetryMinutes { get; init; } = 60;
}
