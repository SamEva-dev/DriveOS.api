using DriveOS.Modules.FundingBilling.Domain.Refunds.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Domain.Refunds;

public sealed class Refund : AggregateRoot<RefundId>, IAuditableEntity
{
    private Refund() { }

    private Refund(RefundId id, OrganizationId organizationId, BillingAccountId billingAccountId, PaymentId paymentId, decimal amount, string currency, string reason, UserId requestedByUserId, DateTimeOffset requestedAtUtc) : base(id)
    {
        OrganizationId = organizationId; BillingAccountId = billingAccountId; PaymentId = paymentId; Amount = Round(amount); Currency = currency; Reason = reason; Status = RefundStatus.Requested; RequestedByUserId = requestedByUserId; RequestedAtUtc = requestedAtUtc.ToUniversalTime();
    }

    public OrganizationId OrganizationId { get; private set; }
    public BillingAccountId BillingAccountId { get; private set; }
    public PaymentId PaymentId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public string Reason { get; private set; } = string.Empty;
    public RefundStatus Status { get; private set; }
    public string? ProviderReference { get; private set; }
    public string? RejectionReason { get; private set; }
    public string? FailureReason { get; private set; }
    public UserId RequestedByUserId { get; private set; }
    public DateTimeOffset RequestedAtUtc { get; private set; }
    public UserId? ApprovedByUserId { get; private set; }
    public DateTimeOffset? ApprovedAtUtc { get; private set; }
    public DateTimeOffset? ProcessingAtUtc { get; private set; }
    public UserId? CompletedByUserId { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }
    public DateTimeOffset? RejectedAtUtc { get; private set; }
    public DateTimeOffset? FailedAtUtc { get; private set; }
    public DateTimeOffset? CancelledAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<Refund> Request(RefundId id, OrganizationId organizationId, BillingAccountId billingAccountId, PaymentId paymentId, decimal amount, string currency, string reason, UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        if (id.IsEmpty) return Result.Failure<Refund>(RefundErrors.InvalidIdentifier);
        if (organizationId.IsEmpty || billingAccountId.IsEmpty || paymentId.IsEmpty) return Result.Failure<Refund>(RefundErrors.InvalidOwner);
        if (amount <= 0m) return Result.Failure<Refund>(RefundErrors.InvalidAmount);
        string c = currency?.Trim().ToUpperInvariant() ?? string.Empty; if (c.Length != 3 || !c.All(x => x is >= 'A' and <= 'Z')) return Result.Failure<Refund>(RefundErrors.InvalidCurrency);
        string r = reason?.Trim() ?? string.Empty; if (r.Length is < 3 or > 1000) return Result.Failure<Refund>(RefundErrors.InvalidReason);
        if (actorUserId.IsEmpty || occurredAtUtc == default) return Result.Failure<Refund>(RefundErrors.InvalidActor);
        var refund = new Refund(id, organizationId, billingAccountId, paymentId, amount, c, r, actorUserId, occurredAtUtc);
        refund.RaiseDomainEvent(new RefundRequestedDomainEvent(refund.Id, refund.PaymentId, refund.BillingAccountId, refund.Amount, refund.Currency, refund.Reason, actorUserId, refund.RequestedAtUtc));
        return Result.Success(refund);
    }

    public Result Approve(UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        if (Status != RefundStatus.Requested) return Result.Failure(RefundErrors.ApprovalNotAllowed);
        if (!ValidActor(actorUserId, occurredAtUtc)) return Result.Failure(RefundErrors.InvalidActor);
        Status = RefundStatus.Approved; ApprovedByUserId = actorUserId; ApprovedAtUtc = occurredAtUtc.ToUniversalTime();
        RaiseDomainEvent(new RefundApprovedDomainEvent(Id, actorUserId, ApprovedAtUtc.Value)); return Result.Success();
    }

    public Result MarkProcessing(UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        if (Status != RefundStatus.Approved) return Result.Failure(RefundErrors.ProcessingNotAllowed);
        if (!ValidActor(actorUserId, occurredAtUtc)) return Result.Failure(RefundErrors.InvalidActor);
        Status = RefundStatus.Processing; ProcessingAtUtc = occurredAtUtc.ToUniversalTime(); return Result.Success();
    }

    public Result Complete(string? providerReference, UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        if (Status is not (RefundStatus.Approved or RefundStatus.Processing)) return Result.Failure(RefundErrors.CompletionNotAllowed);
        if (!ValidActor(actorUserId, occurredAtUtc)) return Result.Failure(RefundErrors.InvalidActor);
        string? reference = string.IsNullOrWhiteSpace(providerReference) ? null : providerReference.Trim(); if (reference?.Length > 250) return Result.Failure(RefundErrors.InvalidProviderReference);
        Status = RefundStatus.Completed; ProviderReference = reference; CompletedByUserId = actorUserId; CompletedAtUtc = occurredAtUtc.ToUniversalTime(); FailureReason = null;
        RaiseDomainEvent(new RefundCompletedDomainEvent(Id, PaymentId, BillingAccountId, Amount, Currency, ProviderReference, actorUserId, CompletedAtUtc.Value)); return Result.Success();
    }

    public Result Reject(string reason, UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        if (Status != RefundStatus.Requested) return Result.Failure(RefundErrors.RejectionNotAllowed);
        string r = reason?.Trim() ?? string.Empty; if (r.Length is < 3 or > 1000) return Result.Failure(RefundErrors.InvalidReason); if (!ValidActor(actorUserId, occurredAtUtc)) return Result.Failure(RefundErrors.InvalidActor);
        Status = RefundStatus.Rejected; RejectionReason = r; RejectedAtUtc = occurredAtUtc.ToUniversalTime(); RaiseDomainEvent(new RefundRejectedDomainEvent(Id, r, actorUserId, RejectedAtUtc.Value)); return Result.Success();
    }

    public Result MarkFailed(string reason, UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        if (Status is not (RefundStatus.Approved or RefundStatus.Processing)) return Result.Failure(RefundErrors.FailureNotAllowed);
        string r = reason?.Trim() ?? string.Empty; if (r.Length is < 3 or > 1000) return Result.Failure(RefundErrors.InvalidReason); if (!ValidActor(actorUserId, occurredAtUtc)) return Result.Failure(RefundErrors.InvalidActor);
        Status = RefundStatus.Failed; FailureReason = r; FailedAtUtc = occurredAtUtc.ToUniversalTime(); RaiseDomainEvent(new RefundFailedDomainEvent(Id, r, actorUserId, FailedAtUtc.Value)); return Result.Success();
    }

    public Result Cancel(UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        if (Status is not (RefundStatus.Requested or RefundStatus.Approved)) return Result.Failure(RefundErrors.CancellationNotAllowed);
        if (!ValidActor(actorUserId, occurredAtUtc)) return Result.Failure(RefundErrors.InvalidActor);
        Status = RefundStatus.Cancelled; CancelledAtUtc = occurredAtUtc.ToUniversalTime(); RaiseDomainEvent(new RefundCancelledDomainEvent(Id, actorUserId, CancelledAtUtc.Value)); return Result.Success();
    }

    public void SetCreatedAudit(DateTimeOffset at, UserId? by) { if (CreatedAtUtc != default) return; CreatedAtUtc = at.ToUniversalTime(); CreatedByUserId = by; }
    public void SetModifiedAudit(DateTimeOffset at, UserId? by) { LastModifiedAtUtc = at.ToUniversalTime(); LastModifiedByUserId = by; }
    private static bool ValidActor(UserId id, DateTimeOffset at) => !id.IsEmpty && at != default;
    private static decimal Round(decimal value) => decimal.Round(value, 2, MidpointRounding.AwayFromZero);
}
