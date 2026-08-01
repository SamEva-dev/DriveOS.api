using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Persistence;

namespace DriveOS.Modules.Organizations.Domain
    .BranchAssignments;

public interface IBranchUserAssignmentRepository :
    IRepository<
        BranchUserAssignment,
        BranchUserAssignmentId>
{
    Task<bool> HasOpenAssignmentAsync(
        OrganizationId organizationId,
        BranchId branchId,
        UserId userId,
        BranchAssignmentRole role,
        CancellationToken cancellationToken = default);

    Task<bool> HasAnotherOpenAssignmentAsync(
        OrganizationId organizationId,
        BranchId branchId,
        UserId userId,
        BranchAssignmentRole role,
        BranchUserAssignmentId excludedAssignmentId,
        CancellationToken cancellationToken = default);

    Task<bool> HasPrimaryAssignmentAsync(
        OrganizationId organizationId,
        UserId userId,
        CancellationToken cancellationToken = default);

    Task<bool> HasAnotherPrimaryAssignmentAsync(
        OrganizationId organizationId,
        UserId userId,
        BranchUserAssignmentId excludedAssignmentId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<
        BranchUserAssignment>>
        GetOpenAssignmentsByUserAsync(
            OrganizationId organizationId,
            UserId userId,
            bool asNoTracking = false,
            CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<
        BranchUserAssignment>>
        GetOpenAssignmentsByBranchAsync(
            OrganizationId organizationId,
            BranchId branchId,
            bool asNoTracking = false,
            CancellationToken cancellationToken = default);
}