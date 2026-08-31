using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.FundingBilling.Application.Persistence;
using DriveOS.Modules.FundingBilling.Application.SupplierPayments;
using DriveOS.Modules.FundingBilling.Domain.SupplierInvoices;
using DriveOS.Modules.ProfessionalMarketplace.Application.Invoices;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Api.Integrations.ProfessionalMarketplace;

internal sealed class ProfessionalInvoiceFinanceGateway(
    ISupplierInvoiceRepository supplierInvoices,
    ISupplierPaymentTimelineReadService paymentTimeline,
    IFundingBillingUnitOfWork fundingUow,
    IClock clock) : IProfessionalInvoiceFinanceGateway
{
    public async Task<ProfessionalInvoiceFinanceSnapshot> EnsureSupplierInvoiceAsync(
        ProfessionalInvoiceFinanceRequest request,
        CancellationToken cancellationToken = default)
    {
        SupplierInvoice? existing=await supplierInvoices.GetByExternalSourceAsync(
            SupplierInvoiceSourceType.ProfessionalMarketplace,
            request.ProfessionalInvoiceId.Value,
            true,
            cancellationToken);

        if(existing is not null)
            return await SnapshotAsync(existing,cancellationToken);

        SupplierInvoiceId id=new(Guid.NewGuid());
        var received=SupplierInvoice.Receive(
            id,
            request.ClientOrganizationId,
            request.ProviderOrganizationId,
            SupplierInvoiceSourceType.ProfessionalMarketplace,
            request.ProfessionalInvoiceId.Value,
            request.ServiceStatementId.Value,
            request.InvoiceNumber,
            request.IssueDate,
            request.DueDate,
            request.Currency,
            request.Subtotal,
            request.TaxAmount,
            request.InvoiceMode,
            clock.UtcNow,
            request.ActorUserId);

        if(received.IsFailure)
            throw new InvalidOperationException($"{received.Error.Code}:{received.Error.Message}");

        // The ProfessionalInvoice is already sourced from an approved ServiceStatement.
        // We can therefore perform the "matching" step automatically, but we deliberately
        // stop before operational and financial approval.
        var matched=received.Value.MarkMatched(request.ActorUserId,clock.UtcNow);
        if(matched.IsFailure)
            throw new InvalidOperationException($"{matched.Error.Code}:{matched.Error.Message}");

        supplierInvoices.Add(received.Value);
        await fundingUow.CommitAsync(cancellationToken);
        return await SnapshotAsync(received.Value,cancellationToken);
    }

    public async Task<ProfessionalInvoiceFinanceSnapshot?> GetSupplierInvoiceAsync(
        ProfessionalInvoiceId professionalInvoiceId,
        CancellationToken cancellationToken = default)
    {
        SupplierInvoice? invoice=await supplierInvoices.GetByExternalSourceAsync(
            SupplierInvoiceSourceType.ProfessionalMarketplace,
            professionalInvoiceId.Value,
            false,
            cancellationToken);

        return invoice is null?null:await SnapshotAsync(invoice,cancellationToken);
    }

    private async Task<ProfessionalInvoiceFinanceSnapshot> SnapshotAsync(
        SupplierInvoice x,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<SupplierPaymentAttemptSnapshot> attempts=await paymentTimeline.ListAsync(x.Id,cancellationToken);
        SupplierPaymentAttemptSnapshot? latest=attempts.OrderByDescending(a=>a.CreatedAtUtc).FirstOrDefault();

        return new ProfessionalInvoiceFinanceSnapshot(
            x.Id.Value,
            x.Status.ToString(),
            x.SettlementStatus.ToString(),
            x.TotalAmount,
            x.PaidAmount,
            x.RefundedAmount,
            x.RemainingAmount,
            x.Currency,
            x.DueDate,
            x.SettlementStatus.ToString(),
            attempts.Select(a=>new ProfessionalInvoicePaymentTimelineItem(
                a.AttemptId,a.Status,a.Amount,a.SettledAmount,a.Currency,a.PaymentMethod,a.ScheduledDate,
                a.SettledOn,a.CreatedAtUtc,a.ProcessingAtUtc,a.PaidAtUtc,a.FailedAtUtc,a.CancelledAtUtc,
                a.ProviderReference,a.FailureReason,a.ReconciliationStatus,a.ReconciliationDifference,
                a.BatchId,a.IsManual)).ToArray());
    }
}
