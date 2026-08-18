using DriveOS.Modules.FundingBilling.Application.Refunds.Read;
using DriveOS.Modules.FundingBilling.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.FundingBilling.Infrastructure.Read;

internal sealed class RefundReadService(FundingBillingDbContext dbContext) : IRefundReadService
{
    public async Task<RefundResponse?> GetAsync(OrganizationId organizationId, RefundId refundId, CancellationToken cancellationToken=default)
    {
        var x=await dbContext.Refunds.AsNoTracking().Where(r=>r.OrganizationId==organizationId&&r.Id==refundId).Select(r=>new {r.Id,r.PaymentId,r.BillingAccountId,r.Amount,r.Currency,r.Reason,r.Status,r.ProviderReference,r.RejectionReason,r.FailureReason,r.RequestedAtUtc,r.ApprovedAtUtc,r.CompletedAtUtc}).SingleOrDefaultAsync(cancellationToken);
        return x is null?null:new RefundResponse(x.Id.Value,x.PaymentId.Value,x.BillingAccountId.Value,x.Amount,x.Currency,x.Reason,x.Status.ToString(),x.ProviderReference,x.RejectionReason,x.FailureReason,x.RequestedAtUtc,x.ApprovedAtUtc,x.CompletedAtUtc);
    }
    public async Task<IReadOnlyCollection<RefundResponse>> ListByPaymentAsync(OrganizationId organizationId, PaymentId paymentId, CancellationToken cancellationToken=default)
    {
        var rows=await dbContext.Refunds.AsNoTracking().Where(r=>r.OrganizationId==organizationId&&r.PaymentId==paymentId).OrderByDescending(r=>r.RequestedAtUtc).Select(r=>new {r.Id,r.PaymentId,r.BillingAccountId,r.Amount,r.Currency,r.Reason,r.Status,r.ProviderReference,r.RejectionReason,r.FailureReason,r.RequestedAtUtc,r.ApprovedAtUtc,r.CompletedAtUtc}).ToArrayAsync(cancellationToken);
        return rows.Select(x=>new RefundResponse(x.Id.Value,x.PaymentId.Value,x.BillingAccountId.Value,x.Amount,x.Currency,x.Reason,x.Status.ToString(),x.ProviderReference,x.RejectionReason,x.FailureReason,x.RequestedAtUtc,x.ApprovedAtUtc,x.CompletedAtUtc)).ToArray();
    }
}
