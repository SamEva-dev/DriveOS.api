using DriveOS.Modules.Contracts.Domain.ContractDocuments;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
namespace DriveOS.Modules.Contracts.Infrastructure.Persistence.Repositories;
internal sealed class ContractDocumentRepository(ContractsDbContext db):IContractDocumentRepository
{ public Task<ContractDocument?> GetByIdAsync(ContractDocumentId id,CancellationToken ct=default)=>db.ContractDocuments.Include(x=>x.Versions).SingleOrDefaultAsync(x=>x.Id==id,ct); public async Task AddAsync(ContractDocument d,CancellationToken ct=default)=>await db.ContractDocuments.AddAsync(d,ct); }
