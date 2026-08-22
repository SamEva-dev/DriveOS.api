namespace DriveOS.Modules.ExamsCertification.Infrastructure.Configuration;

public sealed class ExamAnalyticsOptions
{
    public const string SectionName = "ExamsCertification:Analytics";

    public int SmallSampleThreshold { get; init; } = 10;
    public int DefaultPeriodMonths { get; init; } = 12;
    public decimal PassRateDropAlertPoints { get; init; } = 15m;
    public decimal ContextualUnderperformancePoints { get; init; } = 20m;
    public decimal RecurrentFailureReasonPercent { get; init; } = 35m;
}
