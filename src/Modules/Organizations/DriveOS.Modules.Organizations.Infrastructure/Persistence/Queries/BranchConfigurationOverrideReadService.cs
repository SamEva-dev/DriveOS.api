using DriveOS.Modules.Organizations.Application.BranchConfigurationOverrides;
using DriveOS.Modules.Organizations.Application.BranchConfigurationOverrides.Models;
using DriveOS.Modules.Organizations.Domain.BranchConfigurationOverrides;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Organizations.Infrastructure.Persistence.Queries;

internal sealed class BranchConfigurationOverrideReadService(OrganizationsDbContext dbContext)
    : IBranchConfigurationOverrideReadService
{
    public Task<BranchConfigurationOverrideResponse?> GetByIdAsync(
        OrganizationId organizationId, BranchId branchId, BranchConfigurationOverrideId overrideId,
        CancellationToken cancellationToken = default) =>
        dbContext.BranchConfigurationOverrides
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId && x.BranchId == branchId && x.Id == overrideId)
            .Select(x => new BranchConfigurationOverrideResponse(
                x.Id.Value, x.OrganizationId.Value, x.BranchId.Value, x.BaseConfigurationId.Value,
                x.VersionNumber, x.CountryCode, x.Payload.Json, (int)x.Status,
                x.EffectiveFromUtc, x.EffectiveToUtc, x.PublishedAtUtc,
                x.PublishedByUserId.HasValue ? x.PublishedByUserId.Value.Value : null,
                x.Revision, x.CreatedAtUtc,
                x.CreatedByUserId.HasValue ? x.CreatedByUserId.Value.Value : null,
                x.LastModifiedAtUtc,
                x.LastModifiedByUserId.HasValue ? x.LastModifiedByUserId.Value.Value : null))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<BranchConfigurationOverrideListItemResponse>> GetVersionsAsync(
        OrganizationId organizationId, BranchId branchId, CancellationToken cancellationToken = default) =>
        await dbContext.BranchConfigurationOverrides
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId && x.BranchId == branchId)
            .OrderByDescending(x => x.VersionNumber)
            .Select(x => new BranchConfigurationOverrideListItemResponse(
                x.Id.Value, x.BaseConfigurationId.Value, x.VersionNumber, x.CountryCode, (int)x.Status,
                x.EffectiveFromUtc, x.EffectiveToUtc, x.PublishedAtUtc, x.Revision, x.CreatedAtUtc))
            .ToListAsync(cancellationToken);
}
