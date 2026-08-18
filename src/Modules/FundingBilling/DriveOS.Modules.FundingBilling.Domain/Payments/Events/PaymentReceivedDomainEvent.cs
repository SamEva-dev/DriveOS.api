using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Domain.Payments.Events;

public sealed record PaymentReceivedDomainEvent(
    PaymentId PaymentId,
    BillingAccountId BillingAccountId,
    decimal Amount,
    string Currency,
    string? ExternalReference,
    UserId ActorUserId,
    DateTimeOffset ReceivedAtUtc) : DomainEvent;
