using DriveOS.Modules.FundingBilling.Domain.Payments.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Domain.Payments;

public sealed class Payment : AggregateRoot<PaymentId>, IAuditableEntity
{
    private readonly List<PaymentAllocation> _allocations = [];

    private Payment() { }

    private Payment(
        PaymentId id,
        OrganizationId organizationId,
        BillingAccountId billingAccountId,
        PersonId? payerPersonId,
        OrganizationId? payerOrganizationId,
        decimal amount,
        string currency,
        string paymentMethod,
        string? externalReference)
        : base(id)
    {
        OrganizationId = organizationId;
        BillingAccountId = billingAccountId;
        PayerPersonId = payerPersonId;
        PayerOrganizationId = payerOrganizationId;
        Amount = Round(amount);
        Currency = currency;
        PaymentMethod = paymentMethod;
        ExternalReference = externalReference;
        Status = PaymentStatus.Pending;
    }

    public OrganizationId OrganizationId { get; private set; }
    public BillingAccountId BillingAccountId { get; private set; }
    public PersonId? PayerPersonId { get; private set; }
    public OrganizationId? PayerOrganizationId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public string PaymentMethod { get; private set; } = string.Empty;
    public string? ExternalReference { get; private set; }
    public PaymentStatus Status { get; private set; }
    public IReadOnlyCollection<PaymentAllocation> Allocations => _allocations.AsReadOnly();
    public decimal AllocatedAmount => decimal.Round(_allocations.Sum(x => x.Amount), 2, MidpointRounding.AwayFromZero);
    public decimal UnallocatedAmount => decimal.Max(0m, Amount - AllocatedAmount);
    public decimal RefundedAmount { get; private set; }
    public decimal RefundableAmount => decimal.Max(0m, Amount - RefundedAmount);
    public string? FailureReason { get; private set; }
    public DateTimeOffset? ProcessingAtUtc { get; private set; }
    public DateTimeOffset? PaidAtUtc { get; private set; }
    public DateTimeOffset? FailedAtUtc { get; private set; }
    public DateTimeOffset? CancelledAtUtc { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<Payment> Create(
        PaymentId id,
        OrganizationId organizationId,
        BillingAccountId billingAccountId,
        PersonId? payerPersonId,
        OrganizationId? payerOrganizationId,
        decimal amount,
        string currency,
        string paymentMethod,
        string? externalReference = null)
    {
        if (id.IsEmpty)
            return Result.Failure<Payment>(PaymentErrors.InvalidIdentifier);
        if (organizationId.IsEmpty || billingAccountId.IsEmpty)
            return Result.Failure<Payment>(PaymentErrors.InvalidOwner);

        if (payerPersonId.HasValue && payerPersonId.Value.IsEmpty ||
            payerOrganizationId.HasValue && payerOrganizationId.Value.IsEmpty ||
            payerPersonId.HasValue == payerOrganizationId.HasValue)
            return Result.Failure<Payment>(PaymentErrors.InvalidPayer);

        if (amount <= 0m)
            return Result.Failure<Payment>(PaymentErrors.InvalidAmount);

        string normalizedCurrency = NormalizeCurrency(currency);
        if (!IsValidCurrency(normalizedCurrency))
            return Result.Failure<Payment>(PaymentErrors.InvalidCurrency);

        string normalizedMethod = paymentMethod?.Trim() ?? string.Empty;
        if (normalizedMethod.Length is < 2 or > 80)
            return Result.Failure<Payment>(PaymentErrors.InvalidPaymentMethod);

        string? normalizedReference = string.IsNullOrWhiteSpace(externalReference) ? null : externalReference.Trim();
        if (normalizedReference?.Length > 250)
            return Result.Failure<Payment>(PaymentErrors.InvalidExternalReference);

        var payment = new Payment(
            id, organizationId, billingAccountId, payerPersonId, payerOrganizationId,
            amount, normalizedCurrency, normalizedMethod, normalizedReference);

        payment.RaiseDomainEvent(new PaymentCreatedDomainEvent(
            payment.Id, payment.OrganizationId, payment.BillingAccountId,
            payment.Amount, payment.Currency, payment.PaymentMethod));

        return Result.Success(payment);
    }

    public Result MarkProcessing(UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        if (Status != PaymentStatus.Pending)
            return Result.Failure(PaymentErrors.ProcessingNotAllowed);
        if (!IsValidActor(actorUserId, occurredAtUtc))
            return Result.Failure(PaymentErrors.InvalidActor);

        Status = PaymentStatus.Processing;
        ProcessingAtUtc = occurredAtUtc.ToUniversalTime();
        return Result.Success();
    }

    public Result RecordPaid(string? externalReference, UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        if (Status is not (PaymentStatus.Pending or PaymentStatus.Processing))
            return Result.Failure(PaymentErrors.PaidNotAllowed);
        if (!IsValidActor(actorUserId, occurredAtUtc))
            return Result.Failure(PaymentErrors.InvalidActor);

        string? normalizedReference = string.IsNullOrWhiteSpace(externalReference)
            ? ExternalReference
            : externalReference.Trim();
        if (normalizedReference?.Length > 250)
            return Result.Failure(PaymentErrors.InvalidExternalReference);

        ExternalReference = normalizedReference;
        Status = PaymentStatus.Paid;
        PaidAtUtc = occurredAtUtc.ToUniversalTime();
        FailureReason = null;

        RaiseDomainEvent(new PaymentReceivedDomainEvent(
            Id, BillingAccountId, Amount, Currency, ExternalReference,
            actorUserId, PaidAtUtc.Value));

        return Result.Success();
    }

    public Result MarkFailed(string reason, UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        if (Status is not (PaymentStatus.Pending or PaymentStatus.Processing))
            return Result.Failure(PaymentErrors.FailureNotAllowed);
        if (!IsValidActor(actorUserId, occurredAtUtc))
            return Result.Failure(PaymentErrors.InvalidActor);

        string normalizedReason = reason?.Trim() ?? string.Empty;
        if (normalizedReason.Length is < 3 or > 1000)
            return Result.Failure(PaymentErrors.InvalidFailureReason);

        FailureReason = normalizedReason;
        Status = PaymentStatus.Failed;
        FailedAtUtc = occurredAtUtc.ToUniversalTime();

        RaiseDomainEvent(new PaymentFailedDomainEvent(Id, FailureReason, actorUserId, FailedAtUtc.Value));
        return Result.Success();
    }

    public Result Cancel(UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        if (Status != PaymentStatus.Pending)
            return Result.Failure(PaymentErrors.CancellationNotAllowed);
        if (!IsValidActor(actorUserId, occurredAtUtc))
            return Result.Failure(PaymentErrors.InvalidActor);

        Status = PaymentStatus.Cancelled;
        CancelledAtUtc = occurredAtUtc.ToUniversalTime();
        RaiseDomainEvent(new PaymentCancelledDomainEvent(Id, actorUserId, CancelledAtUtc.Value));
        return Result.Success();
    }


    public Result<PaymentAllocationId> Allocate(
        PaymentAllocationId allocationId,
        InvoiceId? invoiceId,
        PaymentInstallmentId? installmentId,
        decimal amount,
        UserId actorUserId,
        DateTimeOffset occurredAtUtc)
    {
        if (Status != PaymentStatus.Paid)
            return Result.Failure<PaymentAllocationId>(PaymentErrors.AllocationPaymentNotPaid);

        decimal roundedAmount = Round(amount);
        if (roundedAmount <= 0m || roundedAmount > UnallocatedAmount)
            return Result.Failure<PaymentAllocationId>(PaymentErrors.AllocationAmountExceeded);

        Result<PaymentAllocation> allocation = PaymentAllocation.Create(
            allocationId, Id, invoiceId, installmentId, roundedAmount, occurredAtUtc, actorUserId);
        if (allocation.IsFailure)
            return Result.Failure<PaymentAllocationId>(allocation.Error);

        _allocations.Add(allocation.Value);
        RaiseDomainEvent(new PaymentAllocatedDomainEvent(
            Id, allocation.Value.Id, allocation.Value.InvoiceId, allocation.Value.InstallmentId,
            allocation.Value.Amount, actorUserId, allocation.Value.AllocatedAtUtc));

        return Result.Success(allocation.Value.Id);
    }


    public Result RecordRefundCompleted(decimal amount, string currency, UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        if (Status is not (PaymentStatus.Paid or PaymentStatus.PartiallyRefunded))
            return Result.Failure(PaymentErrors.RefundNotAllowed);
        if (!string.Equals(NormalizeCurrency(currency), Currency, StringComparison.Ordinal))
            return Result.Failure(PaymentErrors.CurrencyMismatch);
        decimal roundedAmount = Round(amount);
        if (roundedAmount <= 0m || roundedAmount > RefundableAmount)
            return Result.Failure(PaymentErrors.RefundAmountExceeded);
        if (!IsValidActor(actorUserId, occurredAtUtc))
            return Result.Failure(PaymentErrors.InvalidActor);

        RefundedAmount = Round(RefundedAmount + roundedAmount);
        Status = RefundedAmount >= Amount ? PaymentStatus.Refunded : PaymentStatus.PartiallyRefunded;
        return Result.Success();
    }

    public void SetCreatedAudit(DateTimeOffset createdAtUtc, UserId? createdByUserId)
    {
        if (CreatedAtUtc != default) return;
        CreatedAtUtc = createdAtUtc.ToUniversalTime();
        CreatedByUserId = createdByUserId;
    }

    public void SetModifiedAudit(DateTimeOffset modifiedAtUtc, UserId? modifiedByUserId)
    {
        LastModifiedAtUtc = modifiedAtUtc.ToUniversalTime();
        LastModifiedByUserId = modifiedByUserId;
    }

    private static bool IsValidActor(UserId actorUserId, DateTimeOffset occurredAtUtc) =>
        !actorUserId.IsEmpty && occurredAtUtc != default;
    private static decimal Round(decimal value) => decimal.Round(value, 2, MidpointRounding.AwayFromZero);
    private static string NormalizeCurrency(string currency) => currency?.Trim().ToUpperInvariant() ?? string.Empty;
    private static bool IsValidCurrency(string currency) => currency.Length == 3 && currency.All(c => c is >= 'A' and <= 'Z');
}
