using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Domain.Installments.Events;

public sealed record PaymentInstallmentWaivedDomainEvent(
    PaymentInstallmentId PaymentInstallmentId,
    string Reason,
    UserId ActorUserId,
    DateTimeOffset paymentAtUtc) : DomainEvent;
