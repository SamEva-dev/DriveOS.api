using DriveOS.Application.Abstractions.Pagination;
using DriveOS.Application.Abstractions.Sorting;
using DriveOS.Modules.Organizations.Application.BranchAssignments;
using DriveOS.Modules.Organizations.Application.BranchAssignments.Models;
using DriveOS.Modules.Organizations.Domain.BranchAssignments;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Organizations.Infrastructure.Persistence.Queries;

internal sealed class BranchUserAssignmentReadService(OrganizationsDbContext dbContext)
    : IBranchUserAssignmentReadService
{
    public Task<BranchUserAssignmentItem?> GetByIdAsync(
        OrganizationId organizationId,
        BranchUserAssignmentId assignmentId,
        CancellationToken cancellationToken = default
    )
    {
        return dbContext
            .BranchUserAssignments.AsNoTracking()
            .Where(assignment =>
                assignment.OrganizationId == organizationId && assignment.Id == assignmentId
            )
            .Select(MapItem())
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<BranchUserAssignmentItem>> GetByBranchAsync(
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
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<BranchUserAssignment> query = dbContext
            .BranchUserAssignments.AsNoTracking()
            .Where(assignment =>
                assignment.OrganizationId == organizationId && assignment.BranchId == branchId
            );

        if (!string.IsNullOrWhiteSpace(search))
        {
            string normalized = search.Trim().ToUpperInvariant();

            query = query.Where(assignment =>
                assignment.UserId.Value.ToString().ToUpper().Contains(normalized)
            );
        }

        if (status.HasValue)
        {
            query = query.Where(assignment => assignment.Status == status.Value);
        }

        if (role.HasValue)
        {
            query = query.Where(assignment => assignment.Role == role.Value);
        }

        if (assignmentType.HasValue)
        {
            query = query.Where(assignment => assignment.AssignmentType == assignmentType.Value);
        }

        int totalCount = await query.CountAsync(cancellationToken);

        query = ApplySorting(query, sortBy, sortDirection);

        List<BranchUserAssignmentItem> items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(MapItem())
            .ToListAsync(cancellationToken);

        return new PagedResult<BranchUserAssignmentItem>(items, pageNumber, pageSize, totalCount);
    }

    public async Task<PagedResult<BranchUserAssignmentItem>> GetByUserAsync(
        OrganizationId organizationId,
        UserId userId,
        int pageNumber,
        int pageSize,
        BranchUserAssignmentStatus? status,
        BranchAssignmentRole? role,
        BranchAssignmentType? assignmentType,
        BranchUserAssignmentSortField sortBy,
        SortDirection sortDirection,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<BranchUserAssignment> query = dbContext
            .BranchUserAssignments.AsNoTracking()
            .Where(assignment =>
                assignment.OrganizationId == organizationId && assignment.UserId == userId
            );

        if (status.HasValue)
        {
            query = query.Where(assignment => assignment.Status == status.Value);
        }

        if (role.HasValue)
        {
            query = query.Where(assignment => assignment.Role == role.Value);
        }

        if (assignmentType.HasValue)
        {
            query = query.Where(assignment => assignment.AssignmentType == assignmentType.Value);
        }

        int totalCount = await query.CountAsync(cancellationToken);

        query = ApplySorting(query, sortBy, sortDirection);

        List<BranchUserAssignmentItem> items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(MapItem())
            .ToListAsync(cancellationToken);

        return new PagedResult<BranchUserAssignmentItem>(items, pageNumber, pageSize, totalCount);
    }

    private static IQueryable<BranchUserAssignment> ApplySorting(
        IQueryable<BranchUserAssignment> query,
        BranchUserAssignmentSortField sortBy,
        SortDirection sortDirection
    )
    {
        bool descending = sortDirection == SortDirection.Descending;

        return sortBy switch
        {
            BranchUserAssignmentSortField.UserId => descending
                ? query.OrderByDescending(assignment => assignment.UserId.Value)
                : query.OrderBy(assignment => assignment.UserId.Value),

            BranchUserAssignmentSortField.Role => descending
                ? query.OrderByDescending(assignment => assignment.Role)
                : query.OrderBy(assignment => assignment.Role),

            BranchUserAssignmentSortField.AssignmentType => descending
                ? query.OrderByDescending(assignment => assignment.AssignmentType)
                : query.OrderBy(assignment => assignment.AssignmentType),

            BranchUserAssignmentSortField.Status => descending
                ? query.OrderByDescending(assignment => assignment.Status)
                : query.OrderBy(assignment => assignment.Status),

            BranchUserAssignmentSortField.CreatedAtUtc => descending
                ? query.OrderByDescending(assignment => assignment.CreatedAtUtc)
                : query.OrderBy(assignment => assignment.CreatedAtUtc),

            _ => descending
                ? query.OrderByDescending(assignment => assignment.StartsAtUtc)
                : query.OrderBy(assignment => assignment.StartsAtUtc),
        };
    }

    private static System.Linq.Expressions.Expression<
        Func<BranchUserAssignment, BranchUserAssignmentItem>
    > MapItem()
    {
        return assignment => new BranchUserAssignmentItem(
            assignment.Id.Value,
            assignment.OrganizationId.Value,
            assignment.BranchId.Value,
            assignment.UserId.Value,
            assignment.Role.ToString(),
            assignment.AssignmentType.ToString(),
            assignment.Status.ToString(),
            assignment.StartsAtUtc,
            assignment.PlannedEndAtUtc,
            assignment.EffectiveEndAtUtc,
            assignment.SuspensionReason,
            assignment.SuspendedAtUtc,
            assignment.SuspendedByUserId.HasValue ? assignment.SuspendedByUserId.Value.Value : null,
            assignment.EndReason,
            assignment.EndedAtUtc,
            assignment.EndedByUserId.HasValue ? assignment.EndedByUserId.Value.Value : null,
            assignment.CreatedAtUtc,
            assignment.CreatedByUserId.HasValue ? assignment.CreatedByUserId.Value.Value : null,
            assignment.LastModifiedAtUtc,
            assignment.LastModifiedByUserId.HasValue
                ? assignment.LastModifiedByUserId.Value.Value
                : null
        );
    }
}
