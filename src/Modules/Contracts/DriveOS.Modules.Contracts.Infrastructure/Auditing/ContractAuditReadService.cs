using DriveOS.Modules.Contracts.Application.Auditing;
using DriveOS.Modules.Contracts.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Contracts.Infrastructure.Auditing;

internal sealed class ContractAuditReadService(ContractsDbContext dbContext) : IContractAuditReadService
{
    public async Task<IReadOnlyList<ContractAuditEntryResponse>> ListAsync(
        OrganizationId organizationId,
        TrainingContractId contractId,
        CancellationToken cancellationToken = default) =>
        await dbContext.Set<ContractAuditEntry>()
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId && x.ContractId == contractId)
            .OrderByDescending(x => x.OccurredAtUtc)
            .ThenByDescending(x => x.EventId)
            .Select(x => new ContractAuditEntryResponse(
                x.EventId,
                x.ContractId.Value,
                x.AggregateType,
                x.AggregateId,
                x.Action,
                x.ActorUserId.HasValue ? x.ActorUserId.Value.Value : null,
                x.OccurredAtUtc,
                x.DetailsJson))
            .ToListAsync(cancellationToken);
}
