using DriveOS.Modules.FundingBilling.Domain.Installments.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Domain.Installments;

public sealed class PaymentInstallment : AggregateRoot<PaymentInstallmentId>, IAuditableEntity
{
    private PaymentInstallment() { }

    private PaymentInstallment(
        PaymentInstallmentId id,
        OrganizationId organizationId,
        BillingAccountId billingAccountId,
        DateOnly dueDate,
        decimal expectedAmount,
        string currency,
        PersonId? financingPersonId,
        OrganizationId? financingOrganizationId)
        : base(id)
    {
        OrganizationId = organizationId;
        BillingAccountId = billingAccountId;
        DueDate = dueDate;
        ExpectedAmount = Round(expectedAmount);
        Currency = currency;
        FinancingPersonId = financingPersonId;
        FinancingOrganizationId = financingOrganizationId;
        Status = PaymentInstallmentStatus.Scheduled;
    }

    public OrganizationId OrganizationId { get; private set; }
    public BillingAccountId BillingAccountId { get; private set; }
    public DateOnly DueDate { get; private set; }
    public decimal ExpectedAmount { get; private set; }
    public decimal PaidAmount { get; private set; }
    public decimal RemainingAmount => Status is PaymentInstallmentStatus.Cancelled or PaymentInstallmentStatus.Waived
        ? 0m
        : decimal.Max(0m, ExpectedAmount - PaidAmount);
    public string Currency { get; private set; } = string.Empty;
    public PersonId? FinancingPersonId { get; private set; }
    public OrganizationId? FinancingOrganizationId { get; private set; }
    public PaymentInstallmentStatus Status { get; private set; }
    public DateOnly? PreviousDueDate { get; private set; }
    public string? LastReason { get; private set; }
    public DateTimeOffset? RescheduledAtUtc { get; private set; }
    public DateTimeOffset? CancelledAtUtc { get; private set; }
    public DateTimeOffset? WaivedAtUtc { get; private set; }
    public DateTimeOffset? OverdueAtUtc { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<PaymentInstallment> Create(
        PaymentInstallmentId id,
        OrganizationId organizationId,
        BillingAccountId billingAccountId,
        DateOnly dueDate,
        decimal expectedAmount,
        string currency,
        PersonId? financingPersonId = null,
        OrganizationId? financingOrganizationId = null)
    {
        if (id.IsEmpty)
            return Result.Failure<PaymentInstallment>(PaymentInstallmentErrors.InvalidIdentifier);

        if (organizationId.IsEmpty || billingAccountId.IsEmpty)
            return Result.Failure<PaymentInstallment>(PaymentInstallmentErrors.InvalidOwner);

        if (dueDate == default)
            return Result.Failure<PaymentInstallment>(PaymentInstallmentErrors.InvalidDueDate);

        if (expectedAmount <= 0m)
            return Result.Failure<PaymentInstallment>(PaymentInstallmentErrors.InvalidAmount);

        string normalizedCurrency = NormalizeCurrency(currency);
        if (!IsValidCurrency(normalizedCurrency))
            return Result.Failure<PaymentInstallment>(PaymentInstallmentErrors.InvalidCurrency);

        if (financingPersonId.HasValue && financingPersonId.Value.IsEmpty ||
            financingOrganizationId.HasValue && financingOrganizationId.Value.IsEmpty ||
            financingPersonId.HasValue && financingOrganizationId.HasValue)
            return Result.Failure<PaymentInstallment>(PaymentInstallmentErrors.InvalidFinancingParty);

        var installment = new PaymentInstallment(
            id,
            organizationId,
            billingAccountId,
            dueDate,
            expectedAmount,
            normalizedCurrency,
            financingPersonId,
            financingOrganizationId);

        installment.RaiseDomainEvent(new PaymentInstallmentCreatedDomainEvent(
            installment.Id,
            installment.BillingAccountId,
            installment.DueDate,
            installment.ExpectedAmount,
            installment.Currency));

        return Result.Success(installment);
    }

    public Result Reschedule(DateOnly newDueDate, string reason, UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        if (Status is PaymentInstallmentStatus.Paid or PaymentInstallmentStatus.Cancelled or PaymentInstallmentStatus.Waived)
            return Result.Failure(PaymentInstallmentErrors.ModificationNotAllowed);

        if (newDueDate == default || newDueDate == DueDate)
            return Result.Failure(PaymentInstallmentErrors.InvalidDueDate);

        Result validation = ValidateAction(reason, actorUserId, occurredAtUtc);
        if (validation.IsFailure)
            return validation;

        DateOnly previous = DueDate;
        PreviousDueDate = DueDate;
        DueDate = newDueDate;
        LastReason = reason.Trim();
        Status = PaymentInstallmentStatus.Rescheduled;
        RescheduledAtUtc = occurredAtUtc.ToUniversalTime();

        RaiseDomainEvent(new PaymentInstallmentRescheduledDomainEvent(
            Id, previous, DueDate, LastReason, actorUserId, RescheduledAtUtc.Value));

        return Result.Success();
    }

    public Result Cancel(string reason, UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        if (PaidAmount > 0m || Status is PaymentInstallmentStatus.Paid or PaymentInstallmentStatus.Cancelled or PaymentInstallmentStatus.Waived)
            return Result.Failure(PaymentInstallmentErrors.ModificationNotAllowed);

        Result validation = ValidateAction(reason, actorUserId, occurredAtUtc);
        if (validation.IsFailure)
            return validation;

        LastReason = reason.Trim();
        Status = PaymentInstallmentStatus.Cancelled;
        CancelledAtUtc = occurredAtUtc.ToUniversalTime();
        RaiseDomainEvent(new PaymentInstallmentCancelledDomainEvent(Id, LastReason, actorUserId, CancelledAtUtc.Value));
        return Result.Success();
    }

    public Result Waive(string reason, UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        if (PaidAmount > 0m || Status is PaymentInstallmentStatus.Paid or PaymentInstallmentStatus.Cancelled or PaymentInstallmentStatus.Waived)
            return Result.Failure(PaymentInstallmentErrors.ModificationNotAllowed);

        Result validation = ValidateAction(reason, actorUserId, occurredAtUtc);
        if (validation.IsFailure)
            return validation;

        LastReason = reason.Trim();
        Status = PaymentInstallmentStatus.Waived;
        WaivedAtUtc = occurredAtUtc.ToUniversalTime();
        RaiseDomainEvent(new PaymentInstallmentWaivedDomainEvent(Id, LastReason, actorUserId, WaivedAtUtc.Value));
        return Result.Success();
    }


    public Result MarkOverdue(DateOnly businessDate, DateTimeOffset occurredAtUtc)
    {
        if (Status is not (PaymentInstallmentStatus.Scheduled or PaymentInstallmentStatus.Pending or PaymentInstallmentStatus.Rescheduled or PaymentInstallmentStatus.PartiallyPaid))
            return Result.Failure(PaymentInstallmentErrors.OverdueNotAllowed);
        if (businessDate <= DueDate || RemainingAmount <= 0m || occurredAtUtc == default)
            return Result.Failure(PaymentInstallmentErrors.NotYetOverdue);

        Status = PaymentInstallmentStatus.Overdue;
        OverdueAtUtc = occurredAtUtc.ToUniversalTime();
        RaiseDomainEvent(new PaymentInstallmentOverdueDomainEvent(Id, BillingAccountId, DueDate, RemainingAmount, Currency, OverdueAtUtc.Value));
        return Result.Success();
    }

    public Result RecordPaymentAllocation(decimal amount, string currency, UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        if (Status is PaymentInstallmentStatus.Paid or PaymentInstallmentStatus.Cancelled or PaymentInstallmentStatus.Waived)
            return Result.Failure(PaymentInstallmentErrors.PaymentAllocationNotAllowed);
        if (NormalizeCurrency(currency) != Currency)
            return Result.Failure(PaymentInstallmentErrors.CurrencyMismatch);

        decimal roundedAmount = Round(amount);
        if (roundedAmount <= 0m || roundedAmount > RemainingAmount)
            return Result.Failure(PaymentInstallmentErrors.PaymentAllocationAmountExceeded);
        if (actorUserId.IsEmpty || occurredAtUtc == default)
            return Result.Failure(PaymentInstallmentErrors.InvalidActor);

        bool wasOverdue = Status == PaymentInstallmentStatus.Overdue;
        PaidAmount = Round(PaidAmount + roundedAmount);
        Status = RemainingAmount == 0m ? PaymentInstallmentStatus.Paid : wasOverdue ? PaymentInstallmentStatus.Overdue : PaymentInstallmentStatus.PartiallyPaid;
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

    private static Result ValidateAction(string reason, UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        string normalized = reason?.Trim() ?? string.Empty;
        if (normalized.Length is < 3 or > 1000)
            return Result.Failure(PaymentInstallmentErrors.InvalidReason);
        if (actorUserId.IsEmpty || occurredAtUtc == default)
            return Result.Failure(PaymentInstallmentErrors.InvalidActor);
        return Result.Success();
    }

    private static decimal Round(decimal value) => decimal.Round(value, 2, MidpointRounding.AwayFromZero);
    private static string NormalizeCurrency(string currency) => currency?.Trim().ToUpperInvariant() ?? string.Empty;
    private static bool IsValidCurrency(string currency) => currency.Length == 3 && currency.All(c => c is >= 'A' and <= 'Z');
}
