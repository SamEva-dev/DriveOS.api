using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Domain.Payments.Events;

public sealed record PaymentAllocatedDomainEvent(
    PaymentId PaymentId,
    PaymentAllocationId AllocationId,
    InvoiceId? InvoiceId,
    PaymentInstallmentId? InstallmentId,
    decimal Amount,
    UserId ActorUserId,
    DateTimeOffset AllocatedAtUtc) : DomainEvent;
