using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Application.Invoices.Create;

public sealed record CreateInvoiceLineInput(
    string Description,
    decimal Quantity,
    string Unit,
    decimal UnitPrice,
    decimal DiscountAmount,
    decimal TaxRate);

public sealed record CreateInvoiceCommand(
    OrganizationId OrganizationId,
    BillingAccountId BillingAccountId,
    IReadOnlyCollection<CreateInvoiceLineInput> Lines,
    UserId ActorUserId) : ICommand<InvoiceId>;
