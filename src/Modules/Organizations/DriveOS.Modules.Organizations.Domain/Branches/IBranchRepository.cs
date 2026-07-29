using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Persistence;

namespace DriveOS.Modules.Organizations.Domain.Branches;

public interface IBranchRepository :
    IRepository<Branch, BranchId>
{
    Task<bool> ExistsByNameAsync(
        OrganizationId organizationId,
        string normalizedName,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(
        OrganizationId organizationId,
        string normalizedName,
        BranchId excludedBranchId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByCodeAsync(
        OrganizationId organizationId,
        BranchCode code,
        CancellationToken cancellationToken = default);

    Task<Branch?> GetPrimaryAsync(
        OrganizationId organizationId,
        bool asNoTracking = false,
        CancellationToken cancellationToken = default);
}
