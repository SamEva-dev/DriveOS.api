using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Application.Invoices.Lines;

public sealed record AddInvoiceLineCommand(
    OrganizationId OrganizationId,
    InvoiceId InvoiceId,
    string Description,
    decimal Quantity,
    string Unit,
    decimal UnitPrice,
    decimal DiscountAmount,
    decimal TaxRate,
    UserId ActorUserId) : ICommand<InvoiceLineId>;

public sealed record RemoveInvoiceLineCommand(
    OrganizationId OrganizationId,
    InvoiceId InvoiceId,
    InvoiceLineId InvoiceLineId,
    UserId ActorUserId) : ICommand;
