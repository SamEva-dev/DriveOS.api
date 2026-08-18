using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Application.Invoices.Read;

public sealed record InvoiceLineResponse(
    InvoiceLineId Id,
    string Description,
    decimal Quantity,
    string Unit,
    decimal UnitPrice,
    decimal DiscountAmount,
    decimal TaxRate,
    decimal NetAmount,
    decimal TaxAmount,
    decimal TotalAmount);

public sealed record InvoiceResponse(
    InvoiceId Id,
    BillingAccountId BillingAccountId,
    PersonId CustomerPersonId,
    string Currency,
    string? InvoiceNumber,
    DateOnly? IssueDate,
    DateOnly? DueDate,
    string Status,
    decimal Subtotal,
    decimal TaxAmount,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal CreditedAmount,
    decimal CreditableAmount,
    decimal RemainingAmount,
    DateTimeOffset? IssuedAtUtc,
    IReadOnlyCollection<InvoiceLineResponse> Lines);

public interface IInvoiceReadService
{
    Task<InvoiceResponse?> GetByIdAsync(OrganizationId organizationId, InvoiceId invoiceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InvoiceResponse>> ListByBillingAccountAsync(OrganizationId organizationId, BillingAccountId billingAccountId, CancellationToken cancellationToken = default);
}

public sealed record GetInvoiceQuery(OrganizationId OrganizationId, InvoiceId InvoiceId) : DriveOS.Application.Abstractions.Messaging.IQuery<InvoiceResponse>;
public sealed record GetBillingAccountInvoicesQuery(OrganizationId OrganizationId, BillingAccountId BillingAccountId) : DriveOS.Application.Abstractions.Messaging.IQuery<IReadOnlyCollection<InvoiceResponse>>;
