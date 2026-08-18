namespace DriveOS.Modules.FundingBilling.Domain.Refunds;

public enum RefundStatus
{
    Requested = 0,
    Approved = 1,
    Processing = 2,
    Completed = 3,
    Rejected = 4,
    Failed = 5,
    Cancelled = 6
}
