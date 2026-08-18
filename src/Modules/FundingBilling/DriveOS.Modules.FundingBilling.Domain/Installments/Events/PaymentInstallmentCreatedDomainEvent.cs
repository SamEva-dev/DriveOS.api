using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Domain.Installments.Events;

public sealed record PaymentInstallmentCreatedDomainEvent(
    PaymentInstallmentId PaymentInstallmentId,
    BillingAccountId BillingAccountId,
    DateOnly DueDate,
    decimal ExpectedAmount,
    string Currency) : DomainEvent;
