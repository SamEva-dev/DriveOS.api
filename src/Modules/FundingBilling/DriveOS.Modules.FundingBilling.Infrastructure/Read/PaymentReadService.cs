using DriveOS.Modules.FundingBilling.Application.Payments.Read;
using DriveOS.Modules.FundingBilling.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.FundingBilling.Infrastructure.Read;

internal sealed class PaymentReadService(FundingBillingDbContext dbContext) : IPaymentReadService
{
    public async Task<PaymentResponse?> GetAsync(OrganizationId organizationId, PaymentId paymentId, CancellationToken cancellationToken = default)
    {
        var row = await dbContext.Payments.AsNoTracking().Where(x=>x.OrganizationId==organizationId && x.Id==paymentId)
            .Select(x=>new { x.Id,x.BillingAccountId,x.PayerPersonId,x.PayerOrganizationId,x.Amount,x.RefundedAmount,x.Currency,x.PaymentMethod,x.ExternalReference,x.Status,x.PaidAtUtc,
                Allocations=x.Allocations.OrderBy(a=>a.AllocatedAtUtc).Select(a=>new {a.Id,a.InvoiceId,a.InstallmentId,a.Amount,a.AllocatedAtUtc,a.AllocatedByUserId}).ToArray()})
            .SingleOrDefaultAsync(cancellationToken);
        if(row is null)return null;
        var allocations=row.Allocations.Select(a=>new PaymentAllocationResponse(a.Id.Value,a.InvoiceId?.Value,a.InstallmentId?.Value,a.Amount,a.AllocatedAtUtc,a.AllocatedByUserId.Value)).ToArray();
        decimal allocated=decimal.Round(allocations.Sum(a=>a.Amount),2,MidpointRounding.AwayFromZero);
        return new PaymentResponse(row.Id.Value,row.BillingAccountId.Value,row.PayerPersonId?.Value,row.PayerOrganizationId?.Value,row.Amount,allocated,decimal.Max(0m,row.Amount-allocated),row.RefundedAmount,decimal.Max(0m,row.Amount-row.RefundedAmount),row.Currency,row.PaymentMethod,row.ExternalReference,row.Status.ToString(),row.PaidAtUtc,allocations);
    }

    public async Task<IReadOnlyCollection<PaymentResponse>> ListByBillingAccountAsync(OrganizationId organizationId, BillingAccountId billingAccountId, CancellationToken cancellationToken = default)
    {
        var ids=await dbContext.Payments.AsNoTracking().Where(x=>x.OrganizationId==organizationId&&x.BillingAccountId==billingAccountId).OrderByDescending(x=>x.CreatedAtUtc).Select(x=>x.Id).ToArrayAsync(cancellationToken);
        var result=new List<PaymentResponse>(ids.Length); foreach(var id in ids){var payment=await GetAsync(organizationId,id,cancellationToken);if(payment is not null)result.Add(payment);} return result;
    }
}
