using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Domain.Payments.Events;

public sealed record PaymentCancelledDomainEvent(
    PaymentId PaymentId,
    UserId ActorUserId,
    DateTimeOffset CancelledAtUtc) : DomainEvent;
