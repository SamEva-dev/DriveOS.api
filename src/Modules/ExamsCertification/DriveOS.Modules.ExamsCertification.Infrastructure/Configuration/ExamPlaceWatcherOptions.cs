namespace DriveOS.Modules.ExamsCertification.Infrastructure.Configuration;

public sealed class ExamPlaceWatcherOptions
{
    public const string SectionName = "ExamsCertification:PlaceWatcher";

    public bool Enabled { get; init; } = true;
    public int PollSeconds { get; init; } = 30;
    public int BatchSize { get; init; } = 20;
    public int ProcessingLeaseMinutes { get; init; } = 5;
}
