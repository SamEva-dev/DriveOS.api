using DriveOS.Modules.Organizations.Application.OrganizationSequences;
using DriveOS.Modules.Organizations.Application.OrganizationSequences.Models;
using DriveOS.Modules.Organizations.Domain.OrganizationSequences;
using DriveOS.Modules.Organizations.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Organizations.Infrastructure.OrganizationSequences;

internal sealed class OrganizationSequenceReadService(
    OrganizationsDbContext dbContext)
    : IOrganizationSequenceReadService
{
    public Task<OrganizationSequenceResponse?> GetByIdAsync(
        OrganizationId organizationId,
        OrganizationSequenceId sequenceId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Set<OrganizationSequence>()
            .AsNoTracking()
            .Where(sequence =>
                sequence.OrganizationId == organizationId &&
                sequence.Id == sequenceId)
            .Select(sequence => new OrganizationSequenceResponse(
                sequence.Id.Value,
                sequence.OrganizationId.Value,
                sequence.BranchId.HasValue ? sequence.BranchId.Value.Value : null,
                sequence.Scope.ToString(),
                sequence.Code,
                sequence.Pattern.Value,
                sequence.Padding,
                sequence.NextValue,
                sequence.ResetPolicy.ToString(),
                sequence.LastResetYear,
                sequence.LastResetMonth,
                sequence.Status.ToString(),
                sequence.Revision,
                sequence.CreatedAtUtc,
                sequence.CreatedByUserId.HasValue ? sequence.CreatedByUserId.Value.Value : null,
                sequence.LastModifiedAtUtc,
                sequence.LastModifiedByUserId.HasValue ? sequence.LastModifiedByUserId.Value.Value : null))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<OrganizationSequenceListItem>> GetListAsync(
        OrganizationId organizationId,
        BranchId? branchId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<OrganizationSequence>()
            .AsNoTracking()
            .Where(sequence =>
                sequence.OrganizationId == organizationId &&
                sequence.BranchId == branchId)
            .OrderBy(sequence => sequence.Code)
            .Select(sequence => new OrganizationSequenceListItem(
                sequence.Id.Value,
                sequence.BranchId.HasValue ? sequence.BranchId.Value.Value : null,
                sequence.Scope.ToString(),
                sequence.Code,
                sequence.Pattern.Value,
                sequence.Padding,
                sequence.NextValue,
                sequence.ResetPolicy.ToString(),
                sequence.Status.ToString(),
                sequence.Revision))
            .ToListAsync(cancellationToken);
    }
}
