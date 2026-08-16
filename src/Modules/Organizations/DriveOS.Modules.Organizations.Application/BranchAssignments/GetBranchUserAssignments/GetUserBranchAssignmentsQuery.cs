using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Pagination;
using DriveOS.Application.Abstractions.Sorting;
using DriveOS.Modules.Organizations.Application.BranchAssignments.Models;
using DriveOS.Modules.Organizations.Domain.BranchAssignments;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.BranchAssignments.GetUserBranchAssignments;

public sealed record GetUserBranchAssignmentsQuery(
    OrganizationId OrganizationId,
    UserId UserId,
    int PageNumber,
    int PageSize,
    BranchUserAssignmentStatus? Status,
    BranchAssignmentRole? Role,
    BranchAssignmentType? AssignmentType,
    BranchUserAssignmentSortField SortBy,
    SortDirection SortDirection
) : IQuery<PagedResult<BranchUserAssignmentItem>>;
