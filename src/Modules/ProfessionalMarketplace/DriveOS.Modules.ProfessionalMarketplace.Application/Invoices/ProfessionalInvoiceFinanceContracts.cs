using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.Invoices;

public sealed record ProfessionalInvoiceFinanceRequest(
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
    UserId ActorUserId);

public sealed record ProfessionalInvoiceFinanceSnapshot(
    Guid SupplierInvoiceId,
    string Status,
    string SettlementStatus,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal RefundedAmount,
    decimal RemainingAmount,
    string Currency,
    DateOnly DueDate,
    string? LatestPaymentStatus,
    ProfessionalInvoicePaymentTimelineItem[] PaymentTimeline);

public sealed record ProfessionalInvoicePaymentTimelineItem(
    Guid AttemptId,
    string Status,
    decimal Amount,
    decimal? SettledAmount,
    string Currency,
    string PaymentMethod,
    DateOnly ScheduledDate,
    DateOnly? SettledOn,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ProcessingAtUtc,
    DateTimeOffset? PaidAtUtc,
    DateTimeOffset? FailedAtUtc,
    DateTimeOffset? CancelledAtUtc,
    string? ProviderReference,
    string? FailureReason,
    string ReconciliationStatus,
    decimal? ReconciliationDifference,
    Guid? BatchId,
    bool IsManual);

/// <summary>
/// Anti-corruption port between ProfessionalMarketplace and Finance.
/// BC-13 knows no SupplierInvoice aggregate or FundingBilling persistence type.
/// </summary>
public interface IProfessionalInvoiceFinanceGateway
{
    Task<ProfessionalInvoiceFinanceSnapshot> EnsureSupplierInvoiceAsync(
        ProfessionalInvoiceFinanceRequest request,
        CancellationToken cancellationToken = default);

    Task<ProfessionalInvoiceFinanceSnapshot?> GetSupplierInvoiceAsync(
        ProfessionalInvoiceId professionalInvoiceId,
        CancellationToken cancellationToken = default);
}
