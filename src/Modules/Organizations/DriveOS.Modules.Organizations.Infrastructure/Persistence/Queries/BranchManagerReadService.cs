using DriveOS.Modules.Organizations.Application
    .Branches.Managers;
using DriveOS.Modules.Organizations.Domain.Branches;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Organizations.Infrastructure
    .Persistence.Queries;

internal sealed class BranchManagerReadService(
    OrganizationsDbContext dbContext)
    : IBranchManagerReadService
{
    public Task<BranchManagerAssignmentItem?>
        GetCurrentAsync(
            OrganizationId organizationId,
            BranchId branchId,
            DateTimeOffset atUtc,
            CancellationToken cancellationToken = default)
    {
        return (
            from assignment in
                dbContext
                    .BranchManagerAssignments
                    .AsNoTracking()

            join branch in
                dbContext.Branches
                    .AsNoTracking()
                on assignment.BranchId
                equals branch.Id

            where
                branch.Id == branchId &&
                branch.OrganizationId ==
                    organizationId &&
                branch.Status !=
                    BranchStatus.Closed &&
                assignment.Status ==
                    BranchManagerAssignmentStatus.Active &&
                assignment.EffectiveFromUtc <=
                    atUtc &&
                (
                    assignment.EffectiveToUtc ==
                        null ||
                    assignment.EffectiveToUtc >
                        atUtc
                )

            orderby
                assignment.EffectiveFromUtc
                    descending

            select new
                BranchManagerAssignmentItem(
                    assignment.Id.Value,
                    assignment.BranchId.Value,
                    assignment.ManagerUserId.Value,
                    assignment.EffectiveFromUtc,
                    assignment.EffectiveToUtc,
                    assignment.Status.ToString(),
                    assignment.AssignedByUserId.Value,
                    assignment.AssignedAtUtc,
                    assignment.EndedByUserId
                        .HasValue
                        ? assignment
                            .EndedByUserId
                            .Value
                            .Value
                        : null,
                    assignment.EndedAtUtc)
        ).FirstOrDefaultAsync(
            cancellationToken);
    }

    public async Task<
        IReadOnlyList<
            BranchManagerAssignmentItem>>
        GetHistoryAsync(
            OrganizationId organizationId,
            BranchId branchId,
            CancellationToken cancellationToken = default)
    {
        return await (
            from assignment in
                dbContext
                    .BranchManagerAssignments
                    .AsNoTracking()

            join branch in
                dbContext.Branches
                    .AsNoTracking()
                on assignment.BranchId
                equals branch.Id

            where
                branch.Id == branchId &&
                branch.OrganizationId ==
                    organizationId

            orderby
                assignment.EffectiveFromUtc
                    descending

            select new
                BranchManagerAssignmentItem(
                    assignment.Id.Value,
                    assignment.BranchId.Value,
                    assignment.ManagerUserId.Value,
                    assignment.EffectiveFromUtc,
                    assignment.EffectiveToUtc,
                    assignment.Status.ToString(),
                    assignment.AssignedByUserId.Value,
                    assignment.AssignedAtUtc,
                    assignment.EndedByUserId
                        .HasValue
                        ? assignment
                            .EndedByUserId
                            .Value
                            .Value
                        : null,
                    assignment.EndedAtUtc)
        ).ToListAsync(
            cancellationToken);
    }

    public Task<bool> BranchExistsAsync(
    OrganizationId organizationId,
    BranchId branchId,
    CancellationToken cancellationToken = default)
    {
        return dbContext.Branches
            .AsNoTracking()
            .AnyAsync(
                branch =>
                    branch.Id == branchId &&
                    branch.OrganizationId ==
                        organizationId,
                cancellationToken);
    }
}