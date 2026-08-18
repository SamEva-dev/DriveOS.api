using DriveOS.Modules.FundingBilling.Application.Installments.Read;
using DriveOS.Modules.FundingBilling.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.FundingBilling.Infrastructure.Read;

internal sealed class PaymentInstallmentReadService(FundingBillingDbContext dbContext) : IPaymentInstallmentReadService
{
    public async Task<PaymentInstallmentResponse?> GetByIdAsync(OrganizationId organizationId, PaymentInstallmentId id, CancellationToken cancellationToken = default)
    {
        var row = await dbContext.PaymentInstallments
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId && x.Id == id)
            .Select(x => new
            {
                x.Id, x.BillingAccountId, x.DueDate, x.ExpectedAmount, x.PaidAmount, x.Currency,
                x.FinancingPersonId, x.FinancingOrganizationId, x.Status, x.PreviousDueDate,
                x.LastReason, x.CreatedAtUtc, x.LastModifiedAtUtc
            })
            .SingleOrDefaultAsync(cancellationToken);

        return row is null ? null : new PaymentInstallmentResponse(
            row.Id.Value,
            row.BillingAccountId.Value,
            row.DueDate,
            row.ExpectedAmount,
            row.PaidAmount,
            decimal.Max(0m, row.ExpectedAmount - row.PaidAmount),
            row.Currency,
            row.FinancingPersonId?.Value,
            row.FinancingOrganizationId?.Value,
            row.Status.ToString(),
            row.PreviousDueDate,
            row.LastReason,
            row.CreatedAtUtc,
            row.LastModifiedAtUtc);
    }

    public async Task<IReadOnlyCollection<PaymentInstallmentResponse>> ListByBillingAccountAsync(OrganizationId organizationId, BillingAccountId billingAccountId, CancellationToken cancellationToken = default)
    {
        var rows = await dbContext.PaymentInstallments
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId && x.BillingAccountId == billingAccountId)
            .OrderBy(x => x.DueDate)
            .ThenBy(x => x.CreatedAtUtc)
            .Select(x => new
            {
                x.Id, x.BillingAccountId, x.DueDate, x.ExpectedAmount, x.PaidAmount, x.Currency,
                x.FinancingPersonId, x.FinancingOrganizationId, x.Status, x.PreviousDueDate,
                x.LastReason, x.CreatedAtUtc, x.LastModifiedAtUtc
            })
            .ToListAsync(cancellationToken);

        return rows.Select(row => new PaymentInstallmentResponse(
            row.Id.Value,
            row.BillingAccountId.Value,
            row.DueDate,
            row.ExpectedAmount,
            row.PaidAmount,
            decimal.Max(0m, row.ExpectedAmount - row.PaidAmount),
            row.Currency,
            row.FinancingPersonId?.Value,
            row.FinancingOrganizationId?.Value,
            row.Status.ToString(),
            row.PreviousDueDate,
            row.LastReason,
            row.CreatedAtUtc,
            row.LastModifiedAtUtc)).ToArray();
    }
}
