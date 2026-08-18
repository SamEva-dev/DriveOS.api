namespace DriveOS.Modules.FundingBilling.Domain.FundingPlans;

public enum FundingPlanStatus
{
    Draft = 0,
    PendingApproval = 1,
    PartiallyApproved = 2,
    Approved = 3,
    Rejected = 4,
    Cancelled = 5,
    Closed = 6
}
