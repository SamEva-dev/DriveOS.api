using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.Branches.Managers;

public interface IBranchManagerReadService
{
    Task<bool> BranchExistsAsync(
        OrganizationId organizationId,
        BranchId branchId,
        CancellationToken cancellationToken = default
    );

    Task<BranchManagerAssignmentItem?> GetCurrentAsync(
        OrganizationId organizationId,
        BranchId branchId,
        DateTimeOffset atUtc,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<BranchManagerAssignmentItem>> GetHistoryAsync(
        OrganizationId organizationId,
        BranchId branchId,
        CancellationToken cancellationToken = default
    );
}
