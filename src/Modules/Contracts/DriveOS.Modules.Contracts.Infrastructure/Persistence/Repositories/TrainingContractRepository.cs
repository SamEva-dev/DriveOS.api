using DriveOS.Modules.Contracts.Domain.TrainingContracts;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
namespace DriveOS.Modules.Contracts.Infrastructure.Persistence.Repositories;
internal sealed class TrainingContractRepository(ContractsDbContext db) : ITrainingContractRepository
{
    public Task<TrainingContract?> GetByIdAsync(TrainingContractId contractId, CancellationToken ct = default) =>
        db.TrainingContracts.Include(x => x.Parties).Include(x => x.Signatories).Include(x => x.Versions).ThenInclude(x => x.Parties).SingleOrDefaultAsync(x => x.Id == contractId, ct);
    public Task<TrainingContract?> GetByContractNumberAsync(OrganizationId organizationId, string contractNumber, CancellationToken ct = default) =>
        db.TrainingContracts.AsNoTracking().SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.ContractNumber == contractNumber.Trim().ToUpper(), ct);
    public async Task AddAsync(TrainingContract contract, CancellationToken ct = default) => await db.TrainingContracts.AddAsync(contract, ct);
}
