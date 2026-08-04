using DriveOS.Modules.Organizations.Application.BranchConfigurationOverrides.Models;
using DriveOS.Modules.Organizations.Domain.BranchConfigurationOverrides;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.BranchConfigurationOverrides;

public interface IBranchConfigurationOverrideReadService
{
    Task<BranchConfigurationOverrideResponse?> GetByIdAsync(
        OrganizationId organizationId,
        BranchId branchId,
        BranchConfigurationOverrideId overrideId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BranchConfigurationOverrideListItemResponse>> GetVersionsAsync(
        OrganizationId organizationId,
        BranchId branchId,
        CancellationToken cancellationToken = default);
}
