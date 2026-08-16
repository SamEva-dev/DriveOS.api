using DriveOS.Application.Abstractions.Pagination;
using DriveOS.Application.Abstractions.Sorting;
using DriveOS.Modules.Organizations.Application.Abstractions;
using DriveOS.Modules.Organizations.Application.Organizations.GetOrganizationById;
using DriveOS.Modules.Organizations.Application.Organizations.GetOrganizations;
using DriveOS.Modules.Organizations.Application.Organizations.OrganizationStatusHistory;
using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Organizations.Infrastructure.Persistence.Queries;

internal sealed class OrganizationReadService : IOrganizationReadService
{
    private readonly OrganizationsDbContext _dbContext;

    public OrganizationReadService(OrganizationsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<OrganizationResponse?> GetByIdAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken = default
    )
    {
        return _dbContext
            .Organizations.AsNoTracking()
            .Where(organization => organization.Id == organizationId)
            .Select(organization => new OrganizationResponse(
                organization.Id.Value,
                organization.LegalName,
                organization.CountryCode,
                organization.Type.ToString(),
                organization.Status.ToString(),
                organization.CreatedAtUtc,
                organization.CreatedByUserId.HasValue
                    ? organization.CreatedByUserId.Value.Value
                    : null,
                organization.LastModifiedAtUtc,
                organization.LastModifiedByUserId.HasValue
                    ? organization.LastModifiedByUserId.Value.Value
                    : null
            ))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<OrganizationListItem>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? search,
        OrganizationSortField sortBy,
        SortDirection sortDirection,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<Organization> query = _dbContext.Organizations.AsNoTracking();

        string normalizedSearch = search?.Trim() ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(normalizedSearch))
        {
            string searchPattern = $"%{normalizedSearch}%";

            query = query.Where(organization =>
                EF.Functions.ILike(organization.LegalName, searchPattern)
                || EF.Functions.ILike(organization.CountryCode, searchPattern)
            );
        }

        long totalCount = await query.LongCountAsync(cancellationToken);

        query = ApplySorting(query, sortBy, sortDirection);

        List<OrganizationListItem> items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(organization => new OrganizationListItem(
                organization.Id.Value,
                organization.LegalName,
                organization.CountryCode,
                organization.Type.ToString(),
                organization.Status.ToString(),
                organization.CreatedAtUtc
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<OrganizationListItem>(items, pageNumber, pageSize, totalCount);
    }

    private static IQueryable<Organization> ApplySorting(
        IQueryable<Organization> query,
        OrganizationSortField sortBy,
        SortDirection sortDirection
    )
    {
        bool descending = sortDirection == SortDirection.Descending;

        return sortBy switch
        {
            OrganizationSortField.CountryCode => descending
                ? query.OrderByDescending(organization => organization.CountryCode)
                : query.OrderBy(organization => organization.CountryCode),

            OrganizationSortField.Type => descending
                ? query.OrderByDescending(organization => organization.Type)
                : query.OrderBy(organization => organization.Type),

            OrganizationSortField.Status => descending
                ? query.OrderByDescending(organization => organization.Status)
                : query.OrderBy(organization => organization.Status),

            OrganizationSortField.CreatedAtUtc => descending
                ? query.OrderByDescending(organization => organization.CreatedAtUtc)
                : query.OrderBy(organization => organization.CreatedAtUtc),

            _ => descending
                ? query.OrderByDescending(organization => organization.LegalName)
                : query.OrderBy(organization => organization.LegalName),
        };
    }

    public async Task<IReadOnlyList<OrganizationStatusHistoryItem>> GetStatusHistoryAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken
    )
    {
        return await _dbContext
            .Set<OrganizationStatusHistoryEntry>()
            .AsNoTracking()
            .Where(entry => entry.OrganizationId == organizationId)
            .OrderByDescending(entry => entry.ChangedAtUtc)
            .Select(entry => new OrganizationStatusHistoryItem(
                entry.Id,
                entry.PreviousStatus.ToString(),
                entry.NewStatus.ToString(),
                entry.Reason.Value,
                entry.ChangedByUserId,
                entry.ChangedAtUtc
            ))
            .ToListAsync(cancellationToken);
    }
}
