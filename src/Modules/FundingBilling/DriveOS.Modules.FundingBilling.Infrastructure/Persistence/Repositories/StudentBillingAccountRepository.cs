using DriveOS.Modules.FundingBilling.Domain.BillingAccounts;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
namespace DriveOS.Modules.FundingBilling.Infrastructure.Persistence.Repositories;
public sealed class StudentBillingAccountRepository(FundingBillingDbContext db) : IStudentBillingAccountRepository
{
    public Task<BillingAccount?> GetByIdAsync(BillingAccountId id, CancellationToken ct = default) => db.BillingAccounts.SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<BillingAccount?> GetByStudentAsync(OrganizationId organizationId, PersonId studentId, CancellationToken ct = default) => db.BillingAccounts.SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.StudentId == studentId, ct);
    public Task AddAsync(BillingAccount account, CancellationToken ct = default) => db.BillingAccounts.AddAsync(account, ct).AsTask();
}
