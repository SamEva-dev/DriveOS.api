namespace DriveOS.Modules.ExamsCertification.Infrastructure.Configuration;

public sealed class ExamProviderExecutionOptions
{
    public const string SectionName = "ExamsCertification:ProviderExecution";

    public int DefaultRequestsPerMinute { get; init; } = 60;
    public int MaxRequestsPerMinute { get; init; } = 600;
    public int CircuitFailureThreshold { get; init; } = 5;
    public int CircuitOpenMinutes { get; init; } = 2;
}
