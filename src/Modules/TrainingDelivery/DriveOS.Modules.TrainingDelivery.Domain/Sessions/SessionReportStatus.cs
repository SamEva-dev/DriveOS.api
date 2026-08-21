namespace DriveOS.Modules.TrainingDelivery.Domain.Sessions;

public enum SessionReportStatus
{
    Draft = 0,
    ReadyToSubmit = 1,
    Submitted = 2,
    PendingSupervisorReview = 3,
    Validated = 4,
    RejectedForCorrection = 5
}
