using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.FundingBilling.Domain.Installments.Events;
public sealed record PaymentInstallmentOverdueDomainEvent(PaymentInstallmentId PaymentInstallmentId, BillingAccountId BillingAccountId, DateOnly DueDate, decimal OutstandingAmount, string Currency, DateTimeOffset OverdueAtUtc) : DomainEvent;
