namespace DriveOS.Modules.TrainingDelivery.Domain.Sessions;

public enum TrainingSessionStatus
{
    Scheduled = 1,
    Ready = 2,
    InProgress = 3,
    Completed = 4,
    Cancelled = 5,
    Interrupted = 6
}
