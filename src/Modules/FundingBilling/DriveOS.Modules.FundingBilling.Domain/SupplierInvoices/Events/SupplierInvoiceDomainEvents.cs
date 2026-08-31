using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Domain.SupplierInvoices.Events;

public sealed record SupplierInvoiceReceivedDomainEvent(
    SupplierInvoiceId SupplierInvoiceId,
    OrganizationId ClientOrganizationId,
    Guid SupplierOrganizationId,
    SupplierInvoiceSourceType SourceType,
    Guid ExternalSourceId,
    string? SupplierReference,
    decimal TotalAmount,
    string Currency,
    DateTimeOffset SupplierInvoiceReceivedAtUtc) :DomainEvent;

public sealed record SupplierInvoiceApprovedDomainEvent(
    SupplierInvoiceId SupplierInvoiceId,
    OrganizationId ClientOrganizationId,
    Guid SupplierOrganizationId,
    Guid ExternalSourceId,
    decimal TotalAmount,
    string Currency,
    DateTimeOffset SupplierInvoiceApprovedAtUtc,
    UserId ActorUserId):DomainEvent;
