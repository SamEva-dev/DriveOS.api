using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Domain.Refunds.Events;

public sealed record RefundRequestedDomainEvent(RefundId RefundId, PaymentId PaymentId, BillingAccountId BillingAccountId, decimal Amount, string Currency, string Reason, UserId RequestedByUserId, DateTimeOffset RequestedAtUtc) : DomainEvent;
public sealed record RefundApprovedDomainEvent(RefundId RefundId, UserId ApprovedByUserId, DateTimeOffset ApprovedAtUtc) : DomainEvent;
public sealed record RefundCompletedDomainEvent(RefundId RefundId, PaymentId PaymentId, BillingAccountId BillingAccountId, decimal Amount, string Currency, string? ProviderReference, UserId CompletedByUserId, DateTimeOffset CompletedAtUtc) : DomainEvent;
public sealed record RefundRejectedDomainEvent(RefundId RefundId, string Reason, UserId RejectedByUserId, DateTimeOffset RejectedAtUtc) : DomainEvent;
public sealed record RefundFailedDomainEvent(RefundId RefundId, string Reason, UserId ActorUserId, DateTimeOffset FailedAtUtc) : DomainEvent;
public sealed record RefundCancelledDomainEvent(RefundId RefundId, UserId ActorUserId, DateTimeOffset CancelledAtUtc) : DomainEvent;
