using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Pagination;
using DriveOS.Application.Abstractions.Sorting;
using DriveOS.Modules.Organizations.Application.BranchAssignments.Models;
using DriveOS.Modules.Organizations.Domain.BranchAssignments;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.BranchAssignments.GetBranchUserAssignments;

public sealed record GetBranchUserAssignmentsQuery(
    OrganizationId OrganizationId,
    BranchId BranchId,
    int PageNumber,
    int PageSize,
    string? Search,
    BranchUserAssignmentStatus? Status,
    BranchAssignmentRole? Role,
    BranchAssignmentType? AssignmentType,
    BranchUserAssignmentSortField SortBy,
    SortDirection SortDirection
) : IQuery<PagedResult<BranchUserAssignmentItem>>;
