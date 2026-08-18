namespace DriveOS.Modules.FundingBilling.Domain.Payments;

public enum PaymentStatus
{
    Pending = 0,
    Processing = 1,
    Paid = 2,
    Failed = 3,
    Cancelled = 4,
    PartiallyRefunded = 5,
    Refunded = 6
}
