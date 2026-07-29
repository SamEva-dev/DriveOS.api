using DriveOS.Application.Abstractions.Pagination;
using DriveOS.Application.Abstractions.Sorting;
using DriveOS.Modules.Organizations.Application.Branches;
using DriveOS.Modules.Organizations.Application.Branches.Models;
using DriveOS.Modules.Organizations.Application.Branches.StatusHistory;
using DriveOS.Modules.Organizations.Domain.Branches;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Organizations.Infrastructure
    .Persistence.Queries;

internal sealed class BranchReadService(
    OrganizationsDbContext dbContext)
    : IBranchReadService
{
    public Task<BranchResponse?> GetByIdAsync(
        OrganizationId organizationId,
        BranchId branchId,
        CancellationToken cancellationToken = default) =>
        dbContext.Branches
            .AsNoTracking()
            .Where(branch =>
                branch.OrganizationId == organizationId &&
                branch.Id == branchId)
            .Select(branch => new BranchResponse(
                branch.Id.Value,
                branch.OrganizationId.Value,
                branch.Name.Value,
                branch.Code.Value,
                branch.Type.ToString(),
                branch.Status.ToString(),
                branch.IsPrimary,
                branch.Address.Line1,
                branch.Address.Line2,
                branch.Address.PostalCode,
                branch.Address.City,
                branch.Address.CountryCode,
                branch.TimeZoneId,
                branch.CreatedAtUtc,
                branch.LastModifiedAtUtc))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<PagedResult<BranchListItem>> GetPagedAsync(
        OrganizationId organizationId,
        int pageNumber,
        int pageSize,
        string? search,
        BranchSortField sortBy,
        SortDirection sortDirection,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Branch> query = dbContext.Branches
            .AsNoTracking()
            .Where(branch => branch.OrganizationId == organizationId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            string term = search.Trim();
            string normalized = term.ToUpperInvariant();

            query = query.Where(branch =>
                branch.NormalizedName.Contains(normalized) ||
                branch.Code.Value.Contains(normalized) ||
                EF.Functions.ILike(branch.Address.City, $"%{term}%"));
        }

        int totalCount = await query.CountAsync(cancellationToken);

        query = ApplySorting(query, sortBy, sortDirection);

        List<BranchListItem> items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(branch => new BranchListItem(
                branch.Id.Value,
                branch.Name.Value,
                branch.Code.Value,
                branch.Type.ToString(),
                branch.Status.ToString(),
                branch.IsPrimary,
                branch.Address.City,
                branch.Address.CountryCode,
                branch.TimeZoneId))
            .ToListAsync(cancellationToken);

        return new PagedResult<BranchListItem>(
            items,
            pageNumber,
            pageSize,
            totalCount);
    }

    private static IQueryable<Branch> ApplySorting(
        IQueryable<Branch> query,
        BranchSortField sortBy,
        SortDirection direction)
    {
        bool descending = direction == SortDirection.Descending;

        return sortBy switch
        {
            BranchSortField.Code => descending
                ? query.OrderByDescending(branch => branch.Code.Value)
                : query.OrderBy(branch => branch.Code.Value),
            BranchSortField.City => descending
                ? query.OrderByDescending(branch => branch.Address.City)
                : query.OrderBy(branch => branch.Address.City),
            BranchSortField.BranchType => descending
                ? query.OrderByDescending(branch => branch.Type)
                : query.OrderBy(branch => branch.Type),
            BranchSortField.Status => descending
                ? query.OrderByDescending(branch => branch.Status)
                : query.OrderBy(branch => branch.Status),
            BranchSortField.CreatedAtUtc => descending
                ? query.OrderByDescending(branch => branch.CreatedAtUtc)
                : query.OrderBy(branch => branch.CreatedAtUtc),
            _ => descending
                ? query.OrderByDescending(branch => branch.IsPrimary)
                    .ThenByDescending(branch => branch.NormalizedName)
                : query.OrderByDescending(branch => branch.IsPrimary)
                    .ThenBy(branch => branch.NormalizedName),
        };
    }

    public async Task<
    IReadOnlyList<
        BranchStatusHistoryItem>>
    GetStatusHistoryAsync(
        OrganizationId organizationId,
        BranchId branchId,
        CancellationToken cancellationToken)
    {
        return await dbContext
            .Set<BranchStatusHistoryEntry>()
            .AsNoTracking()
            .Where(entry =>
                entry.BranchId ==
                    new BranchId(branchId.Value))
            .Join(
                dbContext.Branches.AsNoTracking(),
                historyEntry =>
                    historyEntry.BranchId,
                branch =>
                    branch.Id,
                (
                    historyEntry,
                    branch) =>
                    new
                    {
                        HistoryEntry =
                            historyEntry,

                        Branch =
                            branch,
                    })
            .Where(item =>
                item.Branch.OrganizationId ==
                    new OrganizationId(
                        organizationId.Value))
            .OrderByDescending(item =>
                item.HistoryEntry
                    .ChangedAtUtc)
            .Select(item =>
                new BranchStatusHistoryItem(
                    item.HistoryEntry.Id,
                    item.HistoryEntry
                        .PreviousStatus
                        .ToString(),
                    item.HistoryEntry
                        .NewStatus
                        .ToString(),
                    item.HistoryEntry
                        .Reason.Value,
                    item.HistoryEntry
                        .ChangedByUserId,
                    item.HistoryEntry
                        .ChangedAtUtc))
            .ToListAsync(
                cancellationToken);
    }

   
}
