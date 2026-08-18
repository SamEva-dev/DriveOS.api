using DriveOS.Modules.FundingBilling.Domain.Collections.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Domain.Collections;

public sealed class PaymentReminder : AggregateRoot<PaymentReminderId>, IAuditableEntity
{
    private PaymentReminder() { }

    private PaymentReminder(
        PaymentReminderId id,
        OrganizationId organizationId,
        BillingAccountId billingAccountId,
        PaymentReminderTargetType targetType,
        Guid targetId,
        DateOnly dueDate,
        decimal outstandingAmount,
        string currency,
        int sequenceNumber,
        DateTimeOffset requestedAtUtc) : base(id)
    {
        OrganizationId = organizationId;
        BillingAccountId = billingAccountId;
        TargetType = targetType;
        TargetId = targetId;
        DueDate = dueDate;
        OutstandingAmount = Round(outstandingAmount);
        Currency = currency;
        SequenceNumber = sequenceNumber;
        RequestedAtUtc = requestedAtUtc.ToUniversalTime();
        Status = PaymentReminderStatus.Pending;
    }

    public OrganizationId OrganizationId { get; private set; }
    public BillingAccountId BillingAccountId { get; private set; }
    public PaymentReminderTargetType TargetType { get; private set; }
    public Guid TargetId { get; private set; }
    public DateOnly DueDate { get; private set; }
    public decimal OutstandingAmount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public int SequenceNumber { get; private set; }
    public PaymentReminderStatus Status { get; private set; }
    public DateTimeOffset RequestedAtUtc { get; private set; }
    public DateTimeOffset? SentAtUtc { get; private set; }
    public Guid? EmailMessageId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<PaymentReminder> Request(
        PaymentReminderId id,
        OrganizationId organizationId,
        BillingAccountId billingAccountId,
        PaymentReminderTargetType targetType,
        Guid targetId,
        DateOnly dueDate,
        decimal outstandingAmount,
        string currency,
        int sequenceNumber,
        DateTimeOffset requestedAtUtc)
    {
        if (id.IsEmpty) return Result.Failure<PaymentReminder>(PaymentReminderErrors.InvalidIdentifier);
        if (organizationId.IsEmpty || billingAccountId.IsEmpty) return Result.Failure<PaymentReminder>(PaymentReminderErrors.InvalidOwner);
        if (targetId == Guid.Empty || !Enum.IsDefined(targetType)) return Result.Failure<PaymentReminder>(PaymentReminderErrors.InvalidTarget);
        if (dueDate == default) return Result.Failure<PaymentReminder>(PaymentReminderErrors.InvalidDueDate);
        if (outstandingAmount <= 0m || sequenceNumber <= 0) return Result.Failure<PaymentReminder>(PaymentReminderErrors.InvalidAmount);
        string normalizedCurrency = currency?.Trim().ToUpperInvariant() ?? string.Empty;
        if (normalizedCurrency.Length != 3 || normalizedCurrency.Any(c => c is < 'A' or > 'Z')) return Result.Failure<PaymentReminder>(PaymentReminderErrors.InvalidCurrency);
        if (requestedAtUtc == default) return Result.Failure<PaymentReminder>(PaymentReminderErrors.InvalidDueDate);

        var reminder = new PaymentReminder(id, organizationId, billingAccountId, targetType, targetId, dueDate, outstandingAmount, normalizedCurrency, sequenceNumber, requestedAtUtc);
        reminder.RaiseDomainEvent(new PaymentReminderRequestedDomainEvent(id, organizationId, billingAccountId, targetType, targetId, reminder.OutstandingAmount, normalizedCurrency, dueDate, reminder.RequestedAtUtc));
        return Result.Success(reminder);
    }

    public Result MarkSent(Guid emailMessageId, DateTimeOffset sentAtUtc)
    {
        if (Status != PaymentReminderStatus.Pending)
            return Result.Failure(PaymentReminderErrors.InvalidStatus);
        if (emailMessageId == Guid.Empty || sentAtUtc == default)
            return Result.Failure(PaymentReminderErrors.InvalidEmailMessage);

        Status = PaymentReminderStatus.Sent;
        EmailMessageId = emailMessageId;
        SentAtUtc = sentAtUtc.ToUniversalTime();
        return Result.Success();
    }

    public void SetCreatedAudit(DateTimeOffset at, UserId? by) { if (CreatedAtUtc == default) { CreatedAtUtc = at.ToUniversalTime(); CreatedByUserId = by; } }
    public void SetModifiedAudit(DateTimeOffset at, UserId? by) { LastModifiedAtUtc = at.ToUniversalTime(); LastModifiedByUserId = by; }
    private static decimal Round(decimal value) => decimal.Round(value, 2, MidpointRounding.AwayFromZero);
}
