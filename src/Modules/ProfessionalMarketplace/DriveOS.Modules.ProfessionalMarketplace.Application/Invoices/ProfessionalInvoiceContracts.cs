using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Invoices;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.Invoices;

public sealed record CreateProfessionalInvoiceCommand(
    ProfessionalInvoiceId Id,OrganizationId OrganizationId,ServiceStatementId ServiceStatementId,
    ProfessionalInvoiceMode Mode,DateOnly IssueDate,DateOnly DueDate,decimal TaxAmount,
    string? InvoiceNumber,string? BankReference,UserId ActorUserId):ICommand<ProfessionalInvoiceId>;

public sealed record UpdateProfessionalInvoiceDraftCommand(
    ProfessionalInvoiceId Id,ProfessionalProfileId ProfileId,DateOnly IssueDate,DateOnly DueDate,
    decimal TaxAmount,string? InvoiceNumber,string? BankReference,UserId ActorUserId):ICommand;

public sealed record ValidateProfessionalInvoiceCommand(
    ProfessionalInvoiceId Id,ProfessionalProfileId ProfileId,UserId ActorUserId):ICommand;

public sealed record RequestProfessionalInvoiceCommand(
    ProfessionalInvoiceId Id,OrganizationId OrganizationId,UserId ActorUserId):ICommand;

public sealed record SyncProfessionalInvoiceFinanceStatusCommand(
    ProfessionalInvoiceId Id,OrganizationId OrganizationId,UserId ActorUserId):ICommand<ProfessionalInvoiceFinanceSnapshot>;


public sealed record ListOrganizationProfessionalInvoicesQuery(OrganizationId OrganizationId,ProfessionalEngagementId EngagementId)
    :IQuery<IReadOnlyList<ProfessionalInvoiceResponse>>;
public sealed record GetOrganizationProfessionalInvoiceQuery(OrganizationId OrganizationId,ProfessionalInvoiceId Id)
    :IQuery<ProfessionalInvoiceResponse>;

public sealed record ProfessionalInvoiceResponse(
    Guid Id,Guid EngagementId,Guid ProfessionalProfileId,Guid ServiceStatementId,Guid ProviderOrganizationId,Guid ClientOrganizationId,
    string Mode,string? InvoiceNumber,DateOnly IssueDate,DateOnly DueDate,string Currency,decimal Subtotal,decimal TaxAmount,decimal Total,
    string? BankReference,string Status,string PaymentStatus,Guid? FinanceSupplierInvoiceId,string? FinanceSupplierInvoiceStatus,
    DateTimeOffset? FinanceStatusSyncedAtUtc,DateTimeOffset? ValidatedAtUtc,Guid? ValidatedByUserId,DateTimeOffset? RequestedAtUtc,
    DateTimeOffset CreatedAtUtc);

public sealed record UpdateOrganizationProfessionalInvoiceDraftCommand(
    ProfessionalInvoiceId Id,OrganizationId OrganizationId,DateOnly IssueDate,DateOnly DueDate,
    decimal TaxAmount,string? InvoiceNumber,string? BankReference,UserId ActorUserId):ICommand;
public sealed record ValidateOrganizationProfessionalInvoiceCommand(
    ProfessionalInvoiceId Id,OrganizationId OrganizationId,UserId ActorUserId):ICommand;

public sealed record UpdateCurrentProfessionalInvoiceDraftCommand(
    UserId UserId,ProfessionalInvoiceId Id,DateOnly IssueDate,DateOnly DueDate,decimal TaxAmount,string? InvoiceNumber,string? BankReference):ICommand;
public sealed record ValidateCurrentProfessionalInvoiceCommand(UserId UserId,ProfessionalInvoiceId Id):ICommand;
