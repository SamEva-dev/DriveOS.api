namespace DriveOS.Modules.RegulatoryIntegrations.Infrastructure.Reconciliation;
internal sealed class RegulatoryTrainingRecordReconciliationOptions
{
    public bool Enabled { get; init; } = true;
    public int PollSeconds { get; init; } = 300;
    public int BatchSize { get; init; } = 50;
}
