using DriveOS.Modules.FundingBilling.Application.BillingAccounts.Read;
using DriveOS.Modules.FundingBilling.Infrastructure.Persistence;
using DriveOS.Modules.FundingBilling.Domain.BillingAccounts;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
namespace DriveOS.Modules.FundingBilling.Infrastructure.Read;
public sealed class BillingAccountReadService(FundingBillingDbContext db) : IBillingAccountReadService
{
    public async Task<BillingAccountResponse?> GetByIdAsync(OrganizationId organizationId, BillingAccountId id, CancellationToken ct = default)
    {
        var row = await BaseQuery().SingleOrDefaultAsync(x => x.OrganizationId == organizationId.Value && x.Id == id.Value, ct);
        return row is null ? null : ToResponse(row);
    }
    public async Task<BillingAccountResponse?> GetByStudentAsync(OrganizationId organizationId, PersonId studentId, CancellationToken ct = default)
    {
        var row = await BaseQuery().SingleOrDefaultAsync(x => x.OrganizationId == organizationId.Value && x.StudentId == studentId.Value, ct);
        return row is null ? null : ToResponse(row);
    }
    private IQueryable<BillingAccountReadRow> BaseQuery() => db.BillingAccounts.AsNoTracking().Select(x => new BillingAccountReadRow(x.Id.Value, x.OrganizationId.Value, x.StudentId.Value, x.Currency, x.Status, x.TotalInvoiced, x.TotalPaid, x.CreditBalance, x.RestrictionReason, x.SuspensionReason, x.ClosureReason, x.CreatedAtUtc, x.LastModifiedAtUtc));
    private static BillingAccountResponse ToResponse(BillingAccountReadRow x) => new(x.Id, x.OrganizationId, x.StudentId, x.Currency, x.Status.ToString(), x.TotalInvoiced, x.TotalPaid, x.CreditBalance, decimal.Max(0m, x.TotalInvoiced - x.TotalPaid - x.CreditBalance), x.RestrictionReason, x.SuspensionReason, x.ClosureReason, x.CreatedAtUtc, x.LastModifiedAtUtc);
    private sealed record BillingAccountReadRow(Guid Id, Guid OrganizationId, Guid StudentId, string Currency, BillingAccountStatus Status, decimal TotalInvoiced, decimal TotalPaid, decimal CreditBalance, string? RestrictionReason, string? SuspensionReason, string? ClosureReason, DateTimeOffset CreatedAtUtc, DateTimeOffset? LastModifiedAtUtc);
}
