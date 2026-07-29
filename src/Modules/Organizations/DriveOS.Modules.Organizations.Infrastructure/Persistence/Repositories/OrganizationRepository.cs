using System.Linq.Expressions;
using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Organizations.Infrastructure
    .Persistence.Repositories;

internal sealed class OrganizationRepository(
    OrganizationsDbContext dbContext)
    : IOrganizationRepository
{
    public async Task<bool> ExistsByLegalNameAsync(
        string legalName,
        string countryCode,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Organizations
            .AsNoTracking()
            .AnyAsync(
                organization =>
                    organization.LegalName == legalName
                    && organization.CountryCode == countryCode,
                cancellationToken);
    }

    public async Task<Organization?> GetByIdAsync(
        OrganizationId id,
        bool asNoTracking = false,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Organization> query =
            ApplyTracking(
                dbContext.Organizations,
                asNoTracking);

        return await query.SingleOrDefaultAsync(
            organization => organization.Id == id,
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<Organization>> GetAllAsync(
        bool asNoTracking = false,
        CancellationToken cancellationToken = default)
    {
        return await ApplyTracking(
                dbContext.Organizations,
                asNoTracking)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Organization>> FindAsync(
        Expression<Func<Organization, bool>> predicate,
        bool asNoTracking = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        return await ApplyTracking(
                dbContext.Organizations,
                asNoTracking)
            .Where(predicate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Organization?> FirstOrDefaultAsync(
        Expression<Func<Organization, bool>> predicate,
        bool asNoTracking = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        return await ApplyTracking(
                dbContext.Organizations,
                asNoTracking)
            .FirstOrDefaultAsync(
                predicate,
                cancellationToken);
    }

    public async Task<int> CountAsync(
        Expression<Func<Organization, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Organization> query =
            dbContext.Organizations.AsNoTracking();

        return predicate is null
            ? await query.CountAsync(cancellationToken)
            : await query.CountAsync(
                predicate,
                cancellationToken);
    }

    public async Task AddAsync(
        Organization entity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        await dbContext.Organizations.AddAsync(
            entity,
            cancellationToken);
    }

    public void Update(Organization entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        dbContext.Organizations.Update(entity);
    }

    public void Remove(Organization entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        dbContext.Organizations.Remove(entity);
    }

    private static IQueryable<Organization> ApplyTracking(
        IQueryable<Organization> query,
        bool asNoTracking)
    {
        return asNoTracking
            ? query.AsNoTracking()
            : query;
    }
}
