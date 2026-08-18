namespace DriveOS.Modules.FundingBilling.Domain.Installments;

public enum PaymentInstallmentStatus
{
    Scheduled = 0,
    Pending = 1,
    PartiallyPaid = 2,
    Paid = 3,
    Overdue = 4,
    Cancelled = 5,
    Rescheduled = 6,
    Waived = 7
}
