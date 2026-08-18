using DriveOS.Modules.FundingBilling.Domain.BillingAccounts.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Domain.BillingAccounts;

public sealed class BillingAccount : AggregateRoot<BillingAccountId>, IAuditableEntity
{
    private BillingAccount() { }

    private BillingAccount(
        BillingAccountId id,
        OrganizationId organizationId,
        PersonId studentId,
        string currency)
        : base(id)
    {
        OrganizationId = organizationId;
        StudentId = studentId;
        Currency = currency;
        Status = BillingAccountStatus.Open;
    }

    public OrganizationId OrganizationId { get; private set; }
    public PersonId StudentId { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public BillingAccountStatus Status { get; private set; }

    // These monetary totals are deliberately read-only from the outside.
    // Later FIN lots will update them only from invoice/payment/credit workflows.
    public decimal TotalInvoiced { get; private set; }
    public decimal TotalPaid { get; private set; }
    public decimal CreditBalance { get; private set; }
    public decimal OutstandingBalance => decimal.Max(0m, TotalInvoiced - TotalPaid - CreditBalance);

    public string? RestrictionReason { get; private set; }
    public string? SuspensionReason { get; private set; }
    public string? ClosureReason { get; private set; }
    public DateTimeOffset? RestrictedAtUtc { get; private set; }
    public DateTimeOffset? SuspendedAtUtc { get; private set; }
    public DateTimeOffset? ReactivatedAtUtc { get; private set; }
    public DateTimeOffset? ClosedAtUtc { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<BillingAccount> CreateForStudent(
        BillingAccountId id,
        OrganizationId organizationId,
        PersonId studentId,
        string currency)
    {
        if (id.IsEmpty)
            return Result.Failure<BillingAccount>(BillingAccountErrors.InvalidIdentifier);

        if (organizationId.IsEmpty || studentId.IsEmpty)
            return Result.Failure<BillingAccount>(BillingAccountErrors.InvalidOwner);

        string normalizedCurrency = NormalizeCurrency(currency);
        if (!IsValidCurrency(normalizedCurrency))
            return Result.Failure<BillingAccount>(BillingAccountErrors.InvalidCurrency);

        var account = new BillingAccount(id, organizationId, studentId, normalizedCurrency);
        account.RaiseDomainEvent(new BillingAccountCreatedDomainEvent(
            account.Id,
            account.OrganizationId,
            account.StudentId,
            account.Currency));

        return Result.Success(account);
    }

    public Result Restrict(string reason, UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        if (Status != BillingAccountStatus.Open)
            return Result.Failure(BillingAccountErrors.RestrictionNotAllowed);

        Result validation = ValidateAction(reason, actorUserId, occurredAtUtc);
        if (validation.IsFailure)
            return validation;

        RestrictionReason = reason.Trim();
        Status = BillingAccountStatus.Restricted;
        RestrictedAtUtc = occurredAtUtc.ToUniversalTime();

        RaiseDomainEvent(new BillingAccountRestrictedDomainEvent(
            Id,
            RestrictionReason,
            actorUserId,
            RestrictedAtUtc.Value));

        return Result.Success();
    }

    public Result Suspend(string reason, UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        if (Status is not (BillingAccountStatus.Open or BillingAccountStatus.Restricted))
            return Result.Failure(BillingAccountErrors.SuspensionNotAllowed);

        Result validation = ValidateAction(reason, actorUserId, occurredAtUtc);
        if (validation.IsFailure)
            return validation;

        SuspensionReason = reason.Trim();
        Status = BillingAccountStatus.Suspended;
        SuspendedAtUtc = occurredAtUtc.ToUniversalTime();

        RaiseDomainEvent(new BillingAccountSuspendedDomainEvent(
            Id,
            SuspensionReason,
            actorUserId,
            SuspendedAtUtc.Value));

        return Result.Success();
    }

    public Result Reactivate(UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        if (Status is not (BillingAccountStatus.Restricted or BillingAccountStatus.Suspended))
            return Result.Failure(BillingAccountErrors.ReactivationNotAllowed);

        if (actorUserId.IsEmpty || occurredAtUtc == default)
            return Result.Failure(BillingAccountErrors.InvalidOwner);

        RestrictionReason = null;
        SuspensionReason = null;
        Status = BillingAccountStatus.Open;
        ReactivatedAtUtc = occurredAtUtc.ToUniversalTime();

        RaiseDomainEvent(new BillingAccountReactivatedDomainEvent(
            Id,
            actorUserId,
            ReactivatedAtUtc.Value));

        return Result.Success();
    }

    public Result Close(string reason, UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        if (Status == BillingAccountStatus.Closed)
            return Result.Failure(BillingAccountErrors.ClosureNotAllowed);

        Result validation = ValidateAction(reason, actorUserId, occurredAtUtc);
        if (validation.IsFailure)
            return validation;

        ClosureReason = reason.Trim();
        Status = BillingAccountStatus.Closed;
        ClosedAtUtc = occurredAtUtc.ToUniversalTime();

        RaiseDomainEvent(new BillingAccountClosedDomainEvent(
            Id,
            ClosureReason,
            actorUserId,
            ClosedAtUtc.Value));

        return Result.Success();
    }


    public Result RecordInvoiceIssued(decimal amount, string currency, UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        if (Status == BillingAccountStatus.Closed)
            return Result.Failure(BillingAccountErrors.ClosedAccountOperationNotAllowed);

        string normalizedCurrency = NormalizeCurrency(currency);
        if (normalizedCurrency != Currency)
            return Result.Failure(BillingAccountErrors.CurrencyMismatch);

        if (amount < 0m || actorUserId.IsEmpty || occurredAtUtc == default)
            return Result.Failure(BillingAccountErrors.InvalidFinancialOperation);

        TotalInvoiced = decimal.Round(TotalInvoiced + amount, 2, MidpointRounding.AwayFromZero);
        return Result.Success();
    }


    public Result RecordPaymentReceived(decimal amount, string currency, UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        if (Status == BillingAccountStatus.Closed)
            return Result.Failure(BillingAccountErrors.ClosedAccountOperationNotAllowed);
        if (NormalizeCurrency(currency) != Currency)
            return Result.Failure(BillingAccountErrors.CurrencyMismatch);
        if (amount <= 0m || actorUserId.IsEmpty || occurredAtUtc == default)
            return Result.Failure(BillingAccountErrors.InvalidFinancialOperation);

        TotalPaid = decimal.Round(TotalPaid + amount, 2, MidpointRounding.AwayFromZero);
        return Result.Success();
    }


    public Result RecordRefundCompleted(decimal amount, string currency, UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        if (Status == BillingAccountStatus.Closed)
            return Result.Failure(BillingAccountErrors.ClosedAccountOperationNotAllowed);
        if (NormalizeCurrency(currency) != Currency)
            return Result.Failure(BillingAccountErrors.CurrencyMismatch);
        if (amount <= 0m || amount > TotalPaid || actorUserId.IsEmpty || occurredAtUtc == default)
            return Result.Failure(BillingAccountErrors.InvalidFinancialOperation);

        TotalPaid = decimal.Round(TotalPaid - amount, 2, MidpointRounding.AwayFromZero);
        return Result.Success();
    }

    public Result RecordCreditNoteIssued(decimal amount, string currency, UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        if (Status == BillingAccountStatus.Closed)
            return Result.Failure(BillingAccountErrors.ClosedAccountOperationNotAllowed);
        if (NormalizeCurrency(currency) != Currency)
            return Result.Failure(BillingAccountErrors.CurrencyMismatch);
        if (amount <= 0m || amount > TotalInvoiced - CreditBalance || actorUserId.IsEmpty || occurredAtUtc == default)
            return Result.Failure(BillingAccountErrors.InvalidFinancialOperation);
        CreditBalance = decimal.Round(CreditBalance + amount, 2, MidpointRounding.AwayFromZero);
        return Result.Success();
    }

    public void SetCreatedAudit(DateTimeOffset createdAtUtc, UserId? createdByUserId)
    {
        if (CreatedAtUtc != default)
            return;

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
        string normalizedReason = reason?.Trim() ?? string.Empty;
        if (normalizedReason.Length is < 3 or > 1000)
            return Result.Failure(BillingAccountErrors.InvalidReason);

        if (actorUserId.IsEmpty || occurredAtUtc == default)
            return Result.Failure(BillingAccountErrors.InvalidOwner);

        return Result.Success();
    }

    private static string NormalizeCurrency(string currency) => currency?.Trim().ToUpperInvariant() ?? string.Empty;

    private static bool IsValidCurrency(string currency) =>
        currency.Length == 3 && currency.All(character => character is >= 'A' and <= 'Z');
}
