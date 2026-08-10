using DriveOS.Application.Abstractions.Pagination;
using DriveOS.Application.Abstractions.Sorting;
using DriveOS.Modules.CRM.Application.Abstractions.Persistence;
using DriveOS.Modules.CRM.Application.Leads.GetLead;
using DriveOS.Modules.CRM.Application.Leads.GetLeads;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.CRM.Infrastructure.Persistence.Queries;

internal sealed class LeadReadService(CrmDbContext dbContext)
    : ILeadReadService
{
    public Task<LeadResponse?> GetByIdAsync(
        OrganizationId organizationId,
        LeadId leadId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Leads
            .AsNoTracking()
            .Where(lead =>
                lead.OrganizationId == organizationId &&
                lead.Id == leadId)
            .Select(lead => new LeadResponse(
                lead.Id.Value,
                lead.OrganizationId.Value,
                lead.BranchId.HasValue ? lead.BranchId.Value.Value : null,
                lead.Identity.FirstName,
                lead.Identity.LastName,
                lead.Identity.Email,
                lead.Identity.Phone,
                lead.RequestedTraining.LicenseCategory,
                lead.RequestedTraining.Transmission,
                lead.RequestedTraining.PreferredLocation,
                lead.Source.Type,
                lead.Source.Detail,
                lead.AssignedAdvisorId.HasValue
                    ? lead.AssignedAdvisorId.Value.Value
                    : null,
                lead.Status,
                lead.CreatedAtUtc,
                lead.CreatedByUserId.HasValue
                    ? lead.CreatedByUserId.Value.Value
                    : null,
                lead.LastModifiedAtUtc,
                lead.LastModifiedByUserId.HasValue
                    ? lead.LastModifiedByUserId.Value.Value
                    : null))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<LeadListItem>> GetPagedAsync(
        OrganizationId organizationId,
        int pageNumber,
        int pageSize,
        string? search,
        BranchId? branchId,
        LeadStatus? status,
        LeadSourceType? sourceType,
        UserId? assignedAdvisorId,
        bool unassignedOnly,
        LeadSortField sortBy,
        SortDirection sortDirection,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Lead> query = dbContext.Leads
            .AsNoTracking()
            .Where(lead => lead.OrganizationId == organizationId);

        string normalizedSearch = search?.Trim() ?? string.Empty;
        if (normalizedSearch.Length > 0)
        {
            string pattern = $"%{normalizedSearch}%";
            query = query.Where(lead =>
                EF.Functions.ILike(lead.Identity.FirstName, pattern) ||
                EF.Functions.ILike(lead.Identity.LastName, pattern) ||
                (lead.Identity.Email != null &&
                    EF.Functions.ILike(lead.Identity.Email, pattern)) ||
                (lead.Identity.Phone != null &&
                    EF.Functions.ILike(lead.Identity.Phone, pattern)));
        }

        if (branchId.HasValue)
        {
            query = query.Where(lead => lead.BranchId == branchId);
        }

        if (status.HasValue)
        {
            query = query.Where(lead => lead.Status == status.Value);
        }

        if (sourceType.HasValue)
        {
            query = query.Where(lead => lead.Source.Type == sourceType.Value);
        }

        if (unassignedOnly)
        {
            query = query.Where(lead => lead.AssignedAdvisorId == null);
        }
        else if (assignedAdvisorId.HasValue)
        {
            query = query.Where(
                lead => lead.AssignedAdvisorId == assignedAdvisorId);
        }

        long totalCount = await query.LongCountAsync(cancellationToken);

        query = ApplySorting(query, sortBy, sortDirection);

        List<LeadListItem> items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(lead => new LeadListItem(
                lead.Id.Value,
                lead.BranchId.HasValue ? lead.BranchId.Value.Value : null,
                lead.Identity.FirstName,
                lead.Identity.LastName,
                lead.Identity.Email,
                lead.Identity.Phone,
                lead.RequestedTraining.LicenseCategory,
                lead.RequestedTraining.Transmission,
                lead.Source.Type,
                lead.AssignedAdvisorId.HasValue
                    ? lead.AssignedAdvisorId.Value.Value
                    : null,
                lead.Status,
                lead.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return new PagedResult<LeadListItem>(
            items,
            pageNumber,
            pageSize,
            totalCount);
    }

    private static IQueryable<Lead> ApplySorting(
        IQueryable<Lead> query,
        LeadSortField sortBy,
        SortDirection sortDirection)
    {
        bool descending = sortDirection == SortDirection.Descending;

        return sortBy switch
        {
            LeadSortField.LastName => descending
                ? query.OrderByDescending(lead => lead.Identity.LastName)
                    .ThenByDescending(lead => lead.Identity.FirstName)
                : query.OrderBy(lead => lead.Identity.LastName)
                    .ThenBy(lead => lead.Identity.FirstName),

            LeadSortField.FirstName => descending
                ? query.OrderByDescending(lead => lead.Identity.FirstName)
                    .ThenByDescending(lead => lead.Identity.LastName)
                : query.OrderBy(lead => lead.Identity.FirstName)
                    .ThenBy(lead => lead.Identity.LastName),

            LeadSortField.Status => descending
                ? query.OrderByDescending(lead => lead.Status)
                    .ThenByDescending(lead => lead.CreatedAtUtc)
                : query.OrderBy(lead => lead.Status)
                    .ThenByDescending(lead => lead.CreatedAtUtc),

            LeadSortField.SourceType => descending
                ? query.OrderByDescending(lead => lead.Source.Type)
                    .ThenByDescending(lead => lead.CreatedAtUtc)
                : query.OrderBy(lead => lead.Source.Type)
                    .ThenByDescending(lead => lead.CreatedAtUtc),

            LeadSortField.LicenseCategory => descending
                ? query.OrderByDescending(
                        lead => lead.RequestedTraining.LicenseCategory)
                    .ThenByDescending(lead => lead.CreatedAtUtc)
                : query.OrderBy(
                        lead => lead.RequestedTraining.LicenseCategory)
                    .ThenByDescending(lead => lead.CreatedAtUtc),

            _ => descending
                ? query.OrderByDescending(lead => lead.CreatedAtUtc)
                    .ThenByDescending(lead => lead.Id)
                : query.OrderBy(lead => lead.CreatedAtUtc)
                    .ThenBy(lead => lead.Id)
        };
    }
}
