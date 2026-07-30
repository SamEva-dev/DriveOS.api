using System.Linq.Expressions;
using DriveOS.Modules.Organizations.Domain.Branches;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Organizations.Infrastructure.Persistence.Repositories;

internal sealed class BranchRepository(
    OrganizationsDbContext dbContext)
    : IBranchRepository
{
    public Task<bool> ExistsByNameAsync(
        OrganizationId organizationId,
        string normalizedName,
        CancellationToken cancellationToken = default) =>
        dbContext.Branches.AsNoTracking().AnyAsync(
            branch =>
                branch.OrganizationId == organizationId &&
                branch.NormalizedName == normalizedName,
            cancellationToken);

    public Task<bool> ExistsByNameAsync(
        OrganizationId organizationId,
        string normalizedName,
        BranchId excludedBranchId,
        CancellationToken cancellationToken = default) =>
        dbContext.Branches.AsNoTracking().AnyAsync(
            branch =>
                branch.OrganizationId == organizationId &&
                branch.NormalizedName == normalizedName &&
                branch.Id != excludedBranchId,
            cancellationToken);

    public Task<bool> ExistsByCodeAsync(
        OrganizationId organizationId,
        BranchCode code,
        CancellationToken cancellationToken = default) =>
        dbContext.Branches.AsNoTracking().AnyAsync(
            branch =>
                branch.OrganizationId == organizationId &&
                branch.Code == code,
            cancellationToken);

    public Task<Branch?> GetPrimaryAsync(
        OrganizationId organizationId,
        bool asNoTracking = false,
        CancellationToken cancellationToken = default) =>
        ApplyTracking(dbContext.Branches, asNoTracking)
            .SingleOrDefaultAsync(
                branch =>
                    branch.OrganizationId == organizationId &&
                    branch.IsPrimary,
                cancellationToken);

    public Task<Branch?> GetByIdAsync(
    BranchId id,
    bool asNoTracking = false,
    CancellationToken cancellationToken = default)
    {
        IQueryable<Branch> query =
            dbContext.Branches
                .Include(branch =>
                    branch.ManagerAssignments);

        return ApplyTracking(
                query,
                asNoTracking)
            .SingleOrDefaultAsync(
                branch =>
                    branch.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<Branch>> GetAllAsync(
        bool asNoTracking = false,
        CancellationToken cancellationToken = default) =>
        await ApplyTracking(dbContext.Branches, asNoTracking)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<Branch>> FindAsync(
        Expression<Func<Branch, bool>> predicate,
        bool asNoTracking = false,
        CancellationToken cancellationToken = default) =>
        await ApplyTracking(dbContext.Branches, asNoTracking)
            .Where(predicate)
            .ToListAsync(cancellationToken);

    public Task<Branch?> FirstOrDefaultAsync(
        Expression<Func<Branch, bool>> predicate,
        bool asNoTracking = false,
        CancellationToken cancellationToken = default) =>
        ApplyTracking(dbContext.Branches, asNoTracking)
            .FirstOrDefaultAsync(predicate, cancellationToken);

    public Task<int> CountAsync(
        Expression<Func<Branch, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Branch> query = dbContext.Branches.AsNoTracking();

        return predicate is null
            ? query.CountAsync(cancellationToken)
            : query.CountAsync(predicate, cancellationToken);
    }

    public async Task AddAsync(
        Branch entity,
        CancellationToken cancellationToken = default) =>
        await dbContext.Branches.AddAsync(entity, cancellationToken);

    public void Update(Branch entity) => dbContext.Branches.Update(entity);

    public void Remove(Branch entity) => dbContext.Branches.Remove(entity);

    private static IQueryable<Branch> ApplyTracking(
        IQueryable<Branch> query,
        bool asNoTracking) =>
        asNoTracking ? query.AsNoTracking() : query;
}
