using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Students.Application.Reactivations;
using DriveOS.Modules.Students.Application.Suspensions;
using DriveOS.Modules.Students.Domain.Enrollments;
using DriveOS.Modules.Students.Domain.Statuses;
using DriveOS.Modules.Students.Domain.Suspensions;
using DriveOS.Modules.Students.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Students.Infrastructure.Reactivations;

internal sealed class EnrollmentReactivationService(StudentsDbContext db, IClock clock)
    : IEnrollmentReactivationService
{
    public async Task<IReadOnlyList<EnrollmentReactivationResponse>> GetAsync(
        GetEnrollmentReactivationsQuery q,
        CancellationToken ct = default
    )
    {
        var rows = await db
            .EnrollmentReactivations.AsNoTracking()
            .Where(x => x.OrganizationId == q.OrganizationId && x.StudentId == q.StudentId)
            .Include(x => x.Checks)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(ct);
        return rows.Select(Map).ToArray();
    }

    public async Task<Result<Guid>> CreateAsync(
        CreateEnrollmentReactivationCommand c,
        CancellationToken ct = default
    )
    {
        var suspension = await db
            .EnrollmentSuspensions.Include(x => x.History)
            .SingleOrDefaultAsync(
                x =>
                    x.Id == new EnrollmentSuspensionId(c.SuspensionId)
                    && x.OrganizationId == c.OrganizationId
                    && x.StudentId == c.StudentId
                    && x.Status == EnrollmentSuspensionStatus.Active,
                ct
            );
        if (suspension is null)
            return Result.Failure<Guid>(EnrollmentSuspensionErrors.SuspensionNotFound);
        if (
            await db
                .EnrollmentReactivations.AsNoTracking()
                .AnyAsync(
                    x =>
                        x.SuspensionId == new EnrollmentSuspensionId(c.SuspensionId)
                        && x.Status != EnrollmentReactivationStatus.Cancelled,
                    ct
                )
        )
            return Result.Failure<Guid>(EnrollmentSuspensionErrors.ReactivationAlreadyExists);
        var created = EnrollmentReactivation.Create(
            c.OrganizationId,
            c.StudentId,
            suspension.EnrollmentId,
            suspension.Id,
            c.Mode,
            c.ResumeDate,
            c.Conditions,
            c.PedagogyReviewRequested,
            c.ActorUserId,
            clock.UtcNow,
            c.Checks
        );
        if (created.IsFailure)
            return Result.Failure<Guid>(created.Error);
        var reactivation = created.Value;
        db.EnrollmentReactivations.Add(reactivation);
        if (c.Mode == EnrollmentReactivationMode.Immediate)
        {
            var applied = await ApplyCore(reactivation, suspension, c.ActorUserId, ct);
            if (applied.IsFailure)
                return Result.Failure<Guid>(applied.Error);
        }
        await db.SaveChangesAsync(ct);
        return Result.Success(reactivation.Id.Value);
    }

    public async Task<Result> ReviewCheckAsync(
        ReviewEnrollmentReactivationCheckCommand c,
        CancellationToken ct = default
    )
    {
        var x = await db
            .EnrollmentReactivations.Include(r => r.Checks)
            .SingleOrDefaultAsync(
                r =>
                    r.Id == new EnrollmentReactivationId(c.ReactivationId)
                    && r.OrganizationId == c.OrganizationId
                    && r.StudentId == c.StudentId,
                ct
            );
        if (x is null)
            return Result.Failure(EnrollmentSuspensionErrors.ReactivationNotFound);
        var result = x.ReviewCheck(c.Type, c.Status, c.Detail);
        if (result.IsSuccess)
            await db.SaveChangesAsync(ct);
        return result;
    }

    public async Task<Result> ApplyAsync(
        ApplyEnrollmentReactivationCommand c,
        CancellationToken ct = default
    )
    {
        var x = await db
            .EnrollmentReactivations.Include(r => r.Checks)
            .SingleOrDefaultAsync(
                r =>
                    r.Id == new EnrollmentReactivationId(c.ReactivationId)
                    && r.OrganizationId == c.OrganizationId
                    && r.StudentId == c.StudentId,
                ct
            );
        if (x is null)
            return Result.Failure(EnrollmentSuspensionErrors.ReactivationNotFound);
        var suspension = await db
            .EnrollmentSuspensions.Include(s => s.History)
            .SingleOrDefaultAsync(s => s.Id == x.SuspensionId, ct);
        if (suspension is null)
            return Result.Failure(EnrollmentSuspensionErrors.SuspensionNotFound);
        var result = await ApplyCore(x, suspension, c.ActorUserId, ct);
        if (result.IsSuccess)
            await db.SaveChangesAsync(ct);
        return result;
    }

    public async Task ApplyDueAsync(CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);
        var due = await db
            .EnrollmentReactivations.Include(x => x.Checks)
            .Where(x => x.Status == EnrollmentReactivationStatus.Scheduled && x.ResumeDate <= today)
            .ToListAsync(ct);
        foreach (var x in due)
        {
            var suspension = await db
                .EnrollmentSuspensions.Include(s => s.History)
                .SingleOrDefaultAsync(s => s.Id == x.SuspensionId, ct);
            if (suspension is not null)
                await ApplyCore(x, suspension, x.CreatedByUserId, ct);
        }
        if (due.Count > 0)
            await db.SaveChangesAsync(ct);
    }

    private async Task<Result> ApplyCore(
        EnrollmentReactivation x,
        EnrollmentSuspension suspension,
        UserId actor,
        CancellationToken ct
    )
    {
        if (
            !suspension.OperationalBlockId.HasValue
            || suspension.Status != EnrollmentSuspensionStatus.Active
        )
            return Result.Failure(EnrollmentSuspensionErrors.InvalidTransition);
        var board = await db
            .StudentStatusBoards.Include(b => b.Blocks)
            .Include(b => b.History)
            .SingleOrDefaultAsync(
                b => b.OrganizationId == x.OrganizationId && b.StudentId == x.StudentId,
                ct
            );
        if (
            board is null
            || !board.Blocks.Any(b =>
                b.Id == suspension.OperationalBlockId.Value
                && b.Status is StudentBlockStatus.Active or StudentBlockStatus.Overridden
            )
        )
            return Result.Failure(EnrollmentSuspensionErrors.InvalidTransition);
        var enrollment = await db.Enrollments.SingleOrDefaultAsync(
            e => e.Id == x.EnrollmentId && e.OrganizationId == x.OrganizationId,
            ct
        );
        if (enrollment is null)
            return Result.Failure(EnrollmentSuspensionApplicationErrors.ActiveEnrollmentNotFound);
        var applied = x.Apply(DateOnly.FromDateTime(clock.UtcNow.UtcDateTime), clock.UtcNow);
        if (applied.IsFailure)
            return applied;
        var released = board.Release(
            suspension.OperationalBlockId.Value,
            StudentBlockResolutionType.PedagogicalDecision,
            "Validated reactivation checklist",
            actor,
            clock.UtcNow
        );
        if (released.IsFailure)
            return released;
        if (enrollment.Status == EnrollmentStatus.Suspended)
        {
            var resumed = enrollment.Reactivate(actor, clock.UtcNow);
            if (resumed.IsFailure)
                return resumed;
        }
        return suspension.EndForReactivation(actor, clock.UtcNow);
    }

    private static EnrollmentReactivationResponse Map(EnrollmentReactivation x) =>
        new(
            x.Id,
            x.SuspensionId,
            x.EnrollmentId.Value,
            x.Mode,
            x.ResumeDate,
            x.Conditions,
            x.PedagogyReviewRequested,
            x.Status,
            x.AppliedAtUtc,
            x.Checks.OrderBy(c => c.Type)
                .Select(c => new EnrollmentReactivationCheckItem(c.Type, c.Status, c.Detail))
                .ToArray()
        );
}
