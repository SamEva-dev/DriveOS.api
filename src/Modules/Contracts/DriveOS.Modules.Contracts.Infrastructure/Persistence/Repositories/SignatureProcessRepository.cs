using DriveOS.Modules.Contracts.Domain.SignatureProcesses;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Contracts.Infrastructure.Persistence.Repositories;

public sealed class SignatureProcessRepository(ContractsDbContext db) : ISignatureProcessRepository
{
    public async Task AddAsync(SignatureProcess process, CancellationToken cancellationToken = default) =>
        await db.SignatureProcesses.AddAsync(process, cancellationToken);

    public Task<SignatureProcess?> GetByIdAsync(SignatureProcessId id, CancellationToken cancellationToken = default) =>
        db.SignatureProcesses
            .Include(x => x.Recipients)
            .Include(x => x.Evidence)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<bool> ExistsForContractVersionAsync(
        TrainingContractId contractId,
        int contractVersionNumber,
        CancellationToken cancellationToken = default) =>
        db.SignatureProcesses.AsNoTracking().AnyAsync(
            x => x.ContractId == contractId && x.ContractVersionNumber == contractVersionNumber,
            cancellationToken);
}
