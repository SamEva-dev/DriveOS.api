namespace DriveOS.Modules.TrainingDelivery.Domain.Sessions;

public enum SessionReportRevisionStatus
{
    Pending = 1,
    PendingApproval = 2,
    PendingFinancialReview = 3,
    Approved = 4,
    Rejected = 5,
    ResolvedWithoutChange = 6
}
