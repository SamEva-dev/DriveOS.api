using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.FundingBilling.Domain.SupplierInvoices;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Application.SupplierInvoices;

public sealed record ReceiveSupplierInvoiceCommand(
    SupplierInvoiceId Id,
    OrganizationId ClientOrganizationId,
    Guid SupplierOrganizationId,
    SupplierInvoiceSourceType SourceType,
    Guid ExternalSourceId,
    Guid? ServiceStatementId,
    string? SupplierReference,
    DateOnly IssueDate,
    DateOnly DueDate,
    string Currency,
    decimal Subtotal,
    decimal TaxAmount,
    string InvoiceMode,
    UserId ActorUserId) : ICommand<SupplierInvoiceId>;

public sealed record MatchSupplierInvoiceCommand(
    SupplierInvoiceId Id, OrganizationId ClientOrganizationId, UserId ActorUserId) : ICommand;

public sealed record ApproveSupplierInvoiceOperationalCommand(
    SupplierInvoiceId Id, OrganizationId ClientOrganizationId, UserId ActorUserId) : ICommand;

public sealed record ApproveSupplierInvoiceFinancialCommand(
    SupplierInvoiceId Id, OrganizationId ClientOrganizationId, UserId ActorUserId) : ICommand;

public sealed record ScheduleSupplierInvoicePaymentCommand(
    SupplierInvoiceId Id, OrganizationId ClientOrganizationId, UserId ActorUserId) : ICommand;

public sealed record MarkSupplierInvoicePaidCommand(
    SupplierInvoiceId Id, OrganizationId ClientOrganizationId, UserId ActorUserId) : ICommand;

public sealed record RejectSupplierInvoiceCommand(
    SupplierInvoiceId Id, OrganizationId ClientOrganizationId, string Reason, UserId ActorUserId) : ICommand;

public sealed record DisputeSupplierInvoiceCommand(
    SupplierInvoiceId Id, OrganizationId ClientOrganizationId, string Reason, UserId ActorUserId) : ICommand;

public sealed record SupplierInvoiceSnapshot(
    Guid SupplierInvoiceId,
    Guid ExternalSourceId,
    string Status,
    decimal TotalAmount,
    string Currency,
    DateOnly DueDate);

public interface ISupplierInvoiceReadService
{
    Task<SupplierInvoiceSnapshot?> GetByExternalSourceAsync(
        SupplierInvoiceSourceType sourceType,
        Guid externalSourceId,
        CancellationToken ct = default);
}
