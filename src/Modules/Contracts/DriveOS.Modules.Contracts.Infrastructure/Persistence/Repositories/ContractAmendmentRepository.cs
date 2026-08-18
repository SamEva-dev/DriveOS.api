using DriveOS.Modules.Contracts.Domain.ContractAmendments;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Contracts.Infrastructure.Persistence.Repositories;

internal sealed class ContractAmendmentRepository(ContractsDbContext db) : IContractAmendmentRepository
{
    public Task<ContractAmendment?> GetByIdAsync(ContractAmendmentId amendmentId, CancellationToken ct = default) =>
        db.ContractAmendments.SingleOrDefaultAsync(x => x.Id == amendmentId, ct);

    public async Task<int> GetNextNumberAsync(OrganizationId organizationId, TrainingContractId contractId, CancellationToken ct = default)
    {
        int? current = await db.ContractAmendments
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId && x.ContractId == contractId)
            .MaxAsync(x => (int?)x.AmendmentNumber, ct);
        return (current ?? 0) + 1;
    }

    public async Task AddAsync(ContractAmendment amendment, CancellationToken ct = default) =>
        await db.ContractAmendments.AddAsync(amendment, ct);
}
