using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ProfessionalMarketplace.Domain.Events;

public sealed record ProfessionalInvoiceRequestedDomainEvent(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    ProfessionalInvoiceId ProfessionalInvoiceId,
    ServiceStatementId ServiceStatementId,
    Guid ProviderOrganizationId,
    OrganizationId ClientOrganizationId,
    string? InvoiceNumber,
    DateOnly IssueDate,
    DateOnly DueDate,
    string Currency,
    decimal Subtotal,
    decimal TaxAmount,
    decimal Total,
    string InvoiceMode,
    UserId ActorUserId) : IDomainEvent;
