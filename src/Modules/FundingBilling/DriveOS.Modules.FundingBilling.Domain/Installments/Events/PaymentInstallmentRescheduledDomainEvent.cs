using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Domain.Installments.Events;

public sealed record PaymentInstallmentRescheduledDomainEvent(
    PaymentInstallmentId PaymentInstallmentId,
    DateOnly PreviousDueDate,
    DateOnly NewDueDate,
    string Reason,
    UserId ActorUserId,
    DateTimeOffset paymentAtUtc) : DomainEvent;
