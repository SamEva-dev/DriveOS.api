using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.FundingBilling.Domain.Invoices.Events;
public sealed record InvoiceOverdueDomainEvent(InvoiceId InvoiceId, BillingAccountId BillingAccountId, string? InvoiceNumber, DateOnly DueDate, decimal OutstandingAmount, string Currency, DateTimeOffset OverdueAtUtc) : DomainEvent;
