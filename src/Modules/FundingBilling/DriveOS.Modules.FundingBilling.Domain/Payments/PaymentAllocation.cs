using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Domain.Payments;

public sealed class PaymentAllocation
{
    private PaymentAllocation() { }

    private PaymentAllocation(PaymentAllocationId id, PaymentId paymentId, InvoiceId? invoiceId,
        PaymentInstallmentId? installmentId, decimal amount, DateTimeOffset allocatedAtUtc, UserId allocatedByUserId)
    {
        Id = id;
        PaymentId = paymentId;
        InvoiceId = invoiceId;
        InstallmentId = installmentId;
        Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        AllocatedAtUtc = allocatedAtUtc.ToUniversalTime();
        AllocatedByUserId = allocatedByUserId;
    }

    public PaymentAllocationId Id { get; private set; }
    public PaymentId PaymentId { get; private set; }
    public InvoiceId? InvoiceId { get; private set; }
    public PaymentInstallmentId? InstallmentId { get; private set; }
    public decimal Amount { get; private set; }
    public DateTimeOffset AllocatedAtUtc { get; private set; }
    public UserId AllocatedByUserId { get; private set; }

    internal static Result<PaymentAllocation> Create(PaymentAllocationId id, PaymentId paymentId,
        InvoiceId? invoiceId, PaymentInstallmentId? installmentId, decimal amount,
        DateTimeOffset allocatedAtUtc, UserId allocatedByUserId)
    {
        if (id.IsEmpty || paymentId.IsEmpty)
            return Result.Failure<PaymentAllocation>(PaymentErrors.AllocationInvalid);
        if (invoiceId.HasValue == installmentId.HasValue)
            return Result.Failure<PaymentAllocation>(PaymentErrors.AllocationTargetInvalid);
        if (invoiceId is { } inv && inv.IsEmpty || installmentId is { } inst && inst.IsEmpty)
            return Result.Failure<PaymentAllocation>(PaymentErrors.AllocationTargetInvalid);
        if (amount <= 0m || allocatedAtUtc == default || allocatedByUserId.IsEmpty)
            return Result.Failure<PaymentAllocation>(PaymentErrors.AllocationInvalid);

        return Result.Success(new PaymentAllocation(id, paymentId, invoiceId, installmentId, amount, allocatedAtUtc, allocatedByUserId));
    }
}
