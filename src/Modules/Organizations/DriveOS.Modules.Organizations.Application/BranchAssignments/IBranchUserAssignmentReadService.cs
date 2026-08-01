using DriveOS.Application.Abstractions.Pagination;
using DriveOS.Application.Abstractions.Sorting;
using DriveOS.Modules.Organizations.Application
    .BranchAssignments.Models;
using DriveOS.Modules.Organizations.Domain
    .BranchAssignments;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application
    .BranchAssignments;

public interface IBranchUserAssignmentReadService
{
    Task<BranchUserAssignmentItem?>
        GetByIdAsync(
            OrganizationId organizationId,
            BranchUserAssignmentId assignmentId,
            CancellationToken cancellationToken = default);

    Task<PagedResult<
        BranchUserAssignmentItem>>
        GetByBranchAsync(
            OrganizationId organizationId,
            BranchId branchId,
            int pageNumber,
            int pageSize,
            string? search,
            BranchUserAssignmentStatus? status,
            BranchAssignmentRole? role,
            BranchAssignmentType? assignmentType,
            BranchUserAssignmentSortField sortBy,
            SortDirection sortDirection,
            CancellationToken cancellationToken = default);

    Task<PagedResult<
        BranchUserAssignmentItem>>
        GetByUserAsync(
            OrganizationId organizationId,
            UserId userId,
            int pageNumber,
            int pageSize,
            BranchUserAssignmentStatus? status,
            BranchAssignmentRole? role,
            BranchAssignmentType? assignmentType,
            BranchUserAssignmentSortField sortBy,
            SortDirection sortDirection,
            CancellationToken cancellationToken = default);
}