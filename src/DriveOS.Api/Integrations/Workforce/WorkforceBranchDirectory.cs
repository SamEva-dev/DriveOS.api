using DriveOS.Modules.Organizations.Application.Branches;
using DriveOS.Modules.Workforce.Application.BranchAssignments;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Api.Integrations.Workforce;

internal sealed class WorkforceBranchDirectory(IBranchReadService branchReadService) : IWorkforceBranchDirectory
{
    public async Task<WorkforceBranchSnapshot?> GetAsync(OrganizationId organizationId, BranchId branchId, CancellationToken cancellationToken = default)
    {
        var branch = await branchReadService.GetByIdAsync(organizationId, branchId, cancellationToken);
        return branch is null ? null : new WorkforceBranchSnapshot(branch.Id, branch.Name, branch.Code, branch.Status);
    }
}
