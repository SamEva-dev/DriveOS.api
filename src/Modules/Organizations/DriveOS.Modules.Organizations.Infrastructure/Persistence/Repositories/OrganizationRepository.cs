using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Application.Abstractions;
using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Organizations.Infrastructure
    .Persistence.Repositories;

internal sealed class OrganizationRepository :
    IOrganizationRepository
{
    private readonly OrganizationsDbContext _dbContext;

    public OrganizationRepository(
        OrganizationsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> ExistsByLegalNameAsync(
        string legalName,
        string countryCode,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Organizations
            .AsNoTracking()
            .AnyAsync(
                organization =>
                    organization.LegalName == legalName
                    && organization.CountryCode == countryCode,
                cancellationToken);
    }

    public async Task<Organization?> GetByIdAsync(
        OrganizationId organizationId,
        QueryTracking tracking = QueryTracking.NoTracking,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Organization> query =
            _dbContext.Organizations;

        if (tracking == QueryTracking.NoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.SingleOrDefaultAsync(
            organization =>
                organization.Id == organizationId,
            cancellationToken);
    }

    public void Add(Organization organization)
    {
        _dbContext.Organizations.Add(organization);
    }
}