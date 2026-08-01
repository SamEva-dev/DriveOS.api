using System.Linq.Expressions;
using DriveOS.Modules.Organizations.Domain
    .BranchAssignments;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Organizations.Infrastructure
    .Persistence.Repositories;

internal sealed class
    BranchUserAssignmentRepository(
        OrganizationsDbContext dbContext)
    : IBranchUserAssignmentRepository
{
    public Task<bool> HasOpenAssignmentAsync(
        OrganizationId organizationId,
        BranchId branchId,
        UserId userId,
        BranchAssignmentRole role,
        CancellationToken cancellationToken = default)
    {
        return dbContext.BranchUserAssignments
            .AsNoTracking()
            .AnyAsync(
                assignment =>
                    assignment.OrganizationId ==
                        organizationId &&
                    assignment.BranchId ==
                        branchId &&
                    assignment.UserId ==
                        userId &&
                    assignment.Role ==
                        role &&
                    assignment.Status !=
                        BranchUserAssignmentStatus
                            .Ended,
                cancellationToken);
    }

    public Task<bool>
        HasAnotherOpenAssignmentAsync(
            OrganizationId organizationId,
            BranchId branchId,
            UserId userId,
            BranchAssignmentRole role,
            BranchUserAssignmentId excludedAssignmentId,
            CancellationToken cancellationToken = default)
    {
        return dbContext.BranchUserAssignments
            .AsNoTracking()
            .AnyAsync(
                assignment =>
                    assignment.Id !=
                        excludedAssignmentId &&
                    assignment.OrganizationId ==
                        organizationId &&
                    assignment.BranchId ==
                        branchId &&
                    assignment.UserId ==
                        userId &&
                    assignment.Role ==
                        role &&
                    assignment.Status !=
                        BranchUserAssignmentStatus
                            .Ended,
                cancellationToken);
    }

    public Task<bool> HasPrimaryAssignmentAsync(
        OrganizationId organizationId,
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.BranchUserAssignments
            .AsNoTracking()
            .AnyAsync(
                assignment =>
                    assignment.OrganizationId ==
                        organizationId &&
                    assignment.UserId ==
                        userId &&
                    assignment.AssignmentType ==
                        BranchAssignmentType
                            .Primary &&
                    assignment.Status !=
                        BranchUserAssignmentStatus
                            .Ended,
                cancellationToken);
    }

    public Task<bool>
        HasAnotherPrimaryAssignmentAsync(
            OrganizationId organizationId,
            UserId userId,
            BranchUserAssignmentId excludedAssignmentId,
            CancellationToken cancellationToken = default)
    {
        return dbContext.BranchUserAssignments
            .AsNoTracking()
            .AnyAsync(
                assignment =>
                    assignment.Id !=
                        excludedAssignmentId &&
                    assignment.OrganizationId ==
                        organizationId &&
                    assignment.UserId ==
                        userId &&
                    assignment.AssignmentType ==
                        BranchAssignmentType
                            .Primary &&
                    assignment.Status !=
                        BranchUserAssignmentStatus
                            .Ended,
                cancellationToken);
    }

    public async Task<
        IReadOnlyCollection<
            BranchUserAssignment>>
        GetOpenAssignmentsByUserAsync(
            OrganizationId organizationId,
            UserId userId,
            bool asNoTracking = false,
            CancellationToken cancellationToken = default)
    {
        return await ApplyTracking(
                dbContext.BranchUserAssignments,
                asNoTracking)
            .Where(assignment =>
                assignment.OrganizationId ==
                    organizationId &&
                assignment.UserId ==
                    userId &&
                assignment.Status !=
                    BranchUserAssignmentStatus
                        .Ended)
            .ToListAsync(
                cancellationToken);
    }

    public async Task<
        IReadOnlyCollection<
            BranchUserAssignment>>
        GetOpenAssignmentsByBranchAsync(
            OrganizationId organizationId,
            BranchId branchId,
            bool asNoTracking = false,
            CancellationToken cancellationToken = default)
    {
        return await ApplyTracking(
                dbContext.BranchUserAssignments,
                asNoTracking)
            .Where(assignment =>
                assignment.OrganizationId ==
                    organizationId &&
                assignment.BranchId ==
                    branchId &&
                assignment.Status !=
                    BranchUserAssignmentStatus
                        .Ended)
            .ToListAsync(
                cancellationToken);
    }

    public Task<BranchUserAssignment?>
        GetByIdAsync(
            BranchUserAssignmentId id,
            bool asNoTracking = false,
            CancellationToken cancellationToken = default)
    {
        return ApplyTracking(
                dbContext.BranchUserAssignments,
                asNoTracking)
            .SingleOrDefaultAsync(
                assignment =>
                    assignment.Id == id,
                cancellationToken);
    }

    public async Task<
        IReadOnlyCollection<
            BranchUserAssignment>>
        GetAllAsync(
            bool asNoTracking = false,
            CancellationToken cancellationToken = default)
    {
        return await ApplyTracking(
                dbContext.BranchUserAssignments,
                asNoTracking)
            .ToListAsync(
                cancellationToken);
    }

    public async Task<
        IReadOnlyCollection<
            BranchUserAssignment>>
        FindAsync(
            Expression<
                Func<
                    BranchUserAssignment,
                    bool>> predicate,
            bool asNoTracking = false,
            CancellationToken cancellationToken = default)
    {
        return await ApplyTracking(
                dbContext.BranchUserAssignments,
                asNoTracking)
            .Where(predicate)
            .ToListAsync(
                cancellationToken);
    }

    public Task<BranchUserAssignment?>
        FirstOrDefaultAsync(
            Expression<
                Func<
                    BranchUserAssignment,
                    bool>> predicate,
            bool asNoTracking = false,
            CancellationToken cancellationToken = default)
    {
        return ApplyTracking(
                dbContext.BranchUserAssignments,
                asNoTracking)
            .FirstOrDefaultAsync(
                predicate,
                cancellationToken);
    }

    public Task<int> CountAsync(
        Expression<
            Func<
                BranchUserAssignment,
                bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<BranchUserAssignment>
            query =
                dbContext.BranchUserAssignments
                    .AsNoTracking();

        return predicate is null
            ? query.CountAsync(
                cancellationToken)
            : query.CountAsync(
                predicate,
                cancellationToken);
    }

    public async Task AddAsync(
        BranchUserAssignment entity,
        CancellationToken cancellationToken = default)
    {
        await dbContext.BranchUserAssignments
            .AddAsync(
                entity,
                cancellationToken);
    }

    public void Update(
        BranchUserAssignment entity)
    {
        dbContext.BranchUserAssignments
            .Update(entity);
    }

    public void Remove(
        BranchUserAssignment entity)
    {
        dbContext.BranchUserAssignments
            .Remove(entity);
    }

    private static IQueryable<
        BranchUserAssignment>
        ApplyTracking(
            IQueryable<
                BranchUserAssignment> query,
            bool asNoTracking)
    {
        return asNoTracking
            ? query.AsNoTracking()
            : query;
    }
}