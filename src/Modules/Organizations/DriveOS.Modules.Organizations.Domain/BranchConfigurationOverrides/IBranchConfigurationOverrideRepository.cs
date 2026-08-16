using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Domain.BranchConfigurationOverrides;

public interface IBranchConfigurationOverrideRepository
{
    Task<BranchConfigurationOverride?> GetForUpdateAsync(
        BranchConfigurationOverrideId overrideId,
        OrganizationId organizationId,
        BranchId branchId,
        CancellationToken cancellationToken
    );

    Task<bool> VersionExistsAsync(
        OrganizationId organizationId,
        BranchId branchId,
        int versionNumber,
        CancellationToken cancellationToken
    );

    Task AddAsync(BranchConfigurationOverride branchOverride, CancellationToken cancellationToken);
}
