using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Application.Invoices.Issue;

public sealed record IssueInvoiceCommand(
    OrganizationId OrganizationId,
    InvoiceId InvoiceId,
    DateOnly IssueDate,
    DateOnly DueDate,
    UserId ActorUserId) : ICommand<IssueInvoiceResponse>;

public sealed record IssueInvoiceResponse(
    InvoiceId InvoiceId,
    string InvoiceNumber,
    DateOnly IssueDate,
    DateOnly DueDate,
    decimal TotalAmount,
    string Currency);

public interface IInvoiceNumberGenerator
{
    Task<Result<string>> ReserveNextAsync(OrganizationId organizationId, CancellationToken cancellationToken = default);
}

public static class IssueInvoiceErrors
{
    public static readonly Error NumberSequenceNotConfigured = Error.Conflict(
        "FundingBilling.Invoice.NumberSequence.NotConfigured",
        "errors.fundingBilling.invoice.numberSequence.notConfigured");
}
