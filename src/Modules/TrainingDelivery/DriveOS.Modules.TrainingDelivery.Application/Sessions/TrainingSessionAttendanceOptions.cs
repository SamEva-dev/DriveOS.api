namespace DriveOS.Modules.TrainingDelivery.Application.Sessions;

public sealed class TrainingSessionAttendanceOptions
{
    public int RecordingEarlyToleranceMinutes { get; init; } = 30;
    public int CorrectionWindowHours { get; init; } = 24;
}
