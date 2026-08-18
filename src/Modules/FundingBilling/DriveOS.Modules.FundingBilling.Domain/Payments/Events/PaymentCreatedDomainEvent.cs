using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Domain.Payments.Events;

public sealed record PaymentCreatedDomainEvent(
    PaymentId PaymentId,
    OrganizationId OrganizationId,
    BillingAccountId BillingAccountId,
    decimal Amount,
    string Currency,
    string PaymentMethod) : DomainEvent;
