namespace DriveOS.Modules.ExamsCertification.Infrastructure.Configuration;

public sealed class ExamSuccessConsequencesOptions
{
    public const string SectionName = "ExamsCertification:SuccessConsequences";

    public bool Enabled { get; init; } = true;
    public int PollSeconds { get; init; } = 5;
    public int BatchSize { get; init; } = 50;
    public int ProcessingLeaseMinutes { get; init; } = 5;
    public int DeferredRetryHours { get; init; } = 6;
    public int ExceptionRetryMinutes { get; init; } = 5;
    public int MaxRetryMinutes { get; init; } = 60;
}
