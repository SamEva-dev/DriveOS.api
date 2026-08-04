using DriveOS.Modules.Organizations.Domain.BranchConfigurationOverrides;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Organizations.Infrastructure.Persistence.Repositories;

internal sealed class BranchConfigurationOverrideRepository(
    OrganizationsDbContext dbContext) : IBranchConfigurationOverrideRepository
{
    public Task<BranchConfigurationOverride?> GetForUpdateAsync(
        BranchConfigurationOverrideId overrideId,
        OrganizationId organizationId,
        BranchId branchId,
        CancellationToken cancellationToken)
    {
        return dbContext.Set<BranchConfigurationOverride>()
            .SingleOrDefaultAsync(
                branchOverride =>
                    branchOverride.Id == overrideId &&
                    branchOverride.OrganizationId == organizationId &&
                    branchOverride.BranchId == branchId,
                cancellationToken);
    }

    public Task<bool> VersionExistsAsync(
        OrganizationId organizationId,
        BranchId branchId,
        int versionNumber,
        CancellationToken cancellationToken)
    {
        return dbContext.Set<BranchConfigurationOverride>()
            .AsNoTracking()
            .AnyAsync(
                branchOverride =>
                    branchOverride.OrganizationId == organizationId &&
                    branchOverride.BranchId == branchId &&
                    branchOverride.VersionNumber == versionNumber,
                cancellationToken);
    }

    public Task AddAsync(
        BranchConfigurationOverride branchOverride,
        CancellationToken cancellationToken)
    {
        return dbContext.Set<BranchConfigurationOverride>()
            .AddAsync(branchOverride, cancellationToken)
            .AsTask();
    }
}
