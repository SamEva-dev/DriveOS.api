using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Domain.Invoices.Events;

public sealed record InvoiceIssuedDomainEvent(
    InvoiceId InvoiceId,
    BillingAccountId BillingAccountId,
    string InvoiceNumber,
    DateOnly IssueDate,
    DateOnly DueDate,
    decimal TotalAmount,
    string Currency,
    UserId IssuedByUserId,
    DateTimeOffset IssuedAtUtc) : DomainEvent;
