using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Students.Application.Suspensions;
using DriveOS.Modules.Students.Domain.Enrollments;
using DriveOS.Modules.Students.Domain.Statuses;
using DriveOS.Modules.Students.Domain.Suspensions;
using DriveOS.Modules.Students.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Students.Infrastructure.Suspensions;

internal sealed class EnrollmentSuspensionService(StudentsDbContext db, IClock clock)
    : IEnrollmentSuspensionService
{
    public async Task<IReadOnlyList<EnrollmentSuspensionResponse>> GetAsync(
        GetEnrollmentSuspensionsQuery q,
        CancellationToken ct = default
    )
    {
        var rows = await db
            .EnrollmentSuspensions.AsNoTracking()
            .Where(x => x.OrganizationId == q.OrganizationId && x.StudentId == q.StudentId)
            .Include(x => x.History)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(ct);
        return rows.Select(Map).ToArray();
    }

    public async Task<Result<Guid>> SuspendAsync(
        SuspendEnrollmentCommand c,
        CancellationToken ct = default
    )
    {
        if (
            !await db
                .Students.AsNoTracking()
                .AnyAsync(x => x.OrganizationId == c.OrganizationId && x.Id == c.StudentId, ct)
        )
            return Result.Failure<Guid>(EnrollmentSuspensionApplicationErrors.StudentNotFound);
        if (
            await db
                .EnrollmentSuspensions.AsNoTracking()
                .AnyAsync(
                    x =>
                        x.OrganizationId == c.OrganizationId
                        && x.StudentId == c.StudentId
                        && (
                            x.Status == EnrollmentSuspensionStatus.Scheduled
                            || x.Status == EnrollmentSuspensionStatus.Active
                        ),
                    ct
                )
        )
            return Result.Failure<Guid>(EnrollmentSuspensionErrors.ActiveSuspensionExists);
        var enrollment = await db
            .Enrollments.Where(x =>
                x.OrganizationId == c.OrganizationId
                && x.StudentId == c.StudentId
                && x.Status == EnrollmentStatus.Active
            )
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);
        if (enrollment is null)
            return Result.Failure<Guid>(
                EnrollmentSuspensionApplicationErrors.ActiveEnrollmentNotFound
            );
        var created = EnrollmentSuspension.Create(
            c.OrganizationId,
            c.StudentId,
            enrollment.Id,
            c.Reason,
            c.Scope,
            c.StartDate,
            c.ExpectedEndDate,
            c.ImmediateActions,
            c.BookingsDecision,
            c.FutureBookingsCount,
            c.CreditDecision,
            c.NotificationPlan,
            c.ReviewDate,
            c.ActorUserId,
            clock.UtcNow
        );
        if (created.IsFailure)
            return Result.Failure<Guid>(created.Error);
        var suspension = created.Value;
        db.EnrollmentSuspensions.Add(suspension);
        if (suspension.Status == EnrollmentSuspensionStatus.Active)
        {
            var applied = await ApplyOperationalEffects(suspension, enrollment, c.ActorUserId, ct);
            if (applied.IsFailure)
                return Result.Failure<Guid>(applied.Error);
        }
        await db.SaveChangesAsync(ct);
        return Result.Success(suspension.Id.Value);
    }

    public async Task ActivateDueAsync(CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);
        var due = await db
            .EnrollmentSuspensions.Include(x => x.History)
            .Where(x => x.Status == EnrollmentSuspensionStatus.Scheduled && x.StartDate <= today)
            .ToListAsync(ct);
        foreach (var suspension in due)
        {
            var enrollment = await db.Enrollments.SingleOrDefaultAsync(
                x =>
                    x.Id == suspension.EnrollmentId
                    && x.OrganizationId == suspension.OrganizationId,
                ct
            );
            if (enrollment is null)
                continue;
            await ApplyOperationalEffects(suspension, enrollment, suspension.CreatedByUserId, ct);
        }
        if (due.Count > 0)
            await db.SaveChangesAsync(ct);
    }

    private async Task<Result> ApplyOperationalEffects(
        EnrollmentSuspension suspension,
        Enrollment enrollment,
        UserId actor,
        CancellationToken ct
    )
    {
        var board = await db
            .StudentStatusBoards.Include(x => x.Blocks)
            .Include(x => x.History)
            .SingleOrDefaultAsync(
                x =>
                    x.OrganizationId == suspension.OrganizationId
                    && x.StudentId == suspension.StudentId,
                ct
            );
        if (board is null)
        {
            var created = StudentStatusBoard.Create(
                suspension.OrganizationId,
                suspension.StudentId
            );
            if (created.IsFailure)
                return Result.Failure(created.Error);
            board = created.Value;
            db.StudentStatusBoards.Add(board);
        }
        var actions = MapActions(suspension.Scope);
        var block = board.ApplyBlock(
            "EnrollmentSuspension",
            suspension.Reason.ToString(),
            "Students",
            actions,
            StudentBlockSeverity.Blocking,
            $"Review on {suspension.ReviewDate:yyyy-MM-dd}",
            actor,
            clock.UtcNow
        );
        if (block.IsFailure)
            return Result.Failure(block.Error);
        if (suspension.Scope.HasFlag(EnrollmentSuspensionScope.FullEnrollment))
        {
            var stopped = enrollment.Suspend(actor, clock.UtcNow);
            if (stopped.IsFailure)
                return stopped;
        }
        if (suspension.Status == EnrollmentSuspensionStatus.Scheduled)
        {
            var activated = suspension.Activate(block.Value, actor, clock.UtcNow);
            if (activated.IsFailure)
                return activated;
        }
        else
            suspension.AttachBlock(block.Value, actor, clock.UtcNow);
        return Result.Success();
    }

    private static StudentBlockingAction MapActions(EnrollmentSuspensionScope scope)
    {
        if (scope.HasFlag(EnrollmentSuspensionScope.FullEnrollment))
            return StudentBlockingAction.Schedule
                | StudentBlockingAction.StartLesson
                | StudentBlockingAction.Sign
                | StudentBlockingAction.PresentExam
                | StudentBlockingAction.Transfer
                | StudentBlockingAction.Refund
                | StudentBlockingAction.PortalAccess;
        var actions = StudentBlockingAction.None;
        if (scope.HasFlag(EnrollmentSuspensionScope.SchedulingOnly))
            actions |= StudentBlockingAction.Schedule;
        if (scope.HasFlag(EnrollmentSuspensionScope.TrainingDelivery))
            actions |= StudentBlockingAction.StartLesson;
        if (scope.HasFlag(EnrollmentSuspensionScope.ExamRegistration))
            actions |= StudentBlockingAction.PresentExam;
        if (scope.HasFlag(EnrollmentSuspensionScope.PortalAccess))
            actions |= StudentBlockingAction.PortalAccess;
        if (scope.HasFlag(EnrollmentSuspensionScope.FinanceActions))
            actions |= StudentBlockingAction.Refund;
        return actions;
    }

    private static EnrollmentSuspensionResponse Map(EnrollmentSuspension x) =>
        new(
            x.Id,
            x.StudentId.Value,
            x.EnrollmentId.Value,
            x.Reason,
            x.Scope,
            x.StartDate,
            x.ExpectedEndDate,
            x.ImmediateActions,
            x.BookingsDecision,
            x.FutureBookingsCount,
            x.CreditDecision,
            x.NotificationPlan,
            x.ReviewDate,
            x.Status,
            x.NotificationStatus,
            x.OperationalBlockId,
            x.History.OrderBy(h => h.OccurredAtUtc)
                .Select(h => new EnrollmentSuspensionHistoryItem(
                    h.Action,
                    h.Detail,
                    h.ActorUserId.Value,
                    h.OccurredAtUtc
                ))
                .ToArray()
        );
}
