using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Students.Application.Closures;
using DriveOS.Modules.Students.Domain.Closures;
using DriveOS.Modules.Students.Domain.Enrollments;
using DriveOS.Modules.Students.Domain.Statuses;
using DriveOS.Modules.Students.Infrastructure.Persistence;
using DriveOS.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Students.Infrastructure.Closures;

internal sealed class EnrollmentClosureService(StudentsDbContext db, IClock clock)
    : IEnrollmentClosureService
{
    public async Task<IReadOnlyList<EnrollmentClosureResponse>> GetAsync(
        GetEnrollmentClosuresQuery q,
        CancellationToken ct = default
    )
    {
        var rows = await db
            .EnrollmentClosures.AsNoTracking()
            .Where(x => x.OrganizationId == q.OrganizationId && x.StudentId == q.StudentId)
            .Include(x => x.Checks)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(ct);
        return rows.Select(Map).ToArray();
    }

    public async Task<Result<Guid>> CreateAsync(
        CreateEnrollmentClosureCommand c,
        CancellationToken ct = default
    )
    {
        var id = new DriveOS.SharedKernel.Identifiers.DraftEnrollmentId(c.EnrollmentId);
        var enrollment = await db
            .Enrollments.AsNoTracking()
            .SingleOrDefaultAsync(
                x =>
                    x.Id == id
                    && x.OrganizationId == c.OrganizationId
                    && x.StudentId == c.StudentId,
                ct
            );
        if (
            enrollment is null
            || enrollment.Status is not (EnrollmentStatus.Active or EnrollmentStatus.Suspended)
        )
            return Result.Failure<Guid>(EnrollmentClosureErrors.ActiveEnrollmentNotFound);
        if (
            await db
                .EnrollmentClosures.AsNoTracking()
                .AnyAsync(
                    x =>
                        x.EnrollmentId == id
                        && x.Status != EnrollmentClosureStatus.Reopened
                        && x.Status != EnrollmentClosureStatus.Cancelled,
                    ct
                )
        )
            return Result.Failure<Guid>(EnrollmentClosureErrors.AlreadyExists);
        var created = EnrollmentClosureCase.Create(
            c.OrganizationId,
            c.StudentId,
            id,
            enrollment.Status,
            c.Reason,
            c.ClosureDate,
            c.ReasonDetail,
            c.ActorUserId,
            clock.UtcNow,
            c.Checks
        );
        if (created.IsFailure)
            return Result.Failure<Guid>(created.Error);
        db.EnrollmentClosures.Add(created.Value);
        await db.SaveChangesAsync(ct);
        return Result.Success(created.Value.Id.Value);
    }

    public async Task<Result> ReviewCheckAsync(
        ReviewEnrollmentClosureCheckCommand c,
        CancellationToken ct = default
    )
    {
        var x = await Find(c.OrganizationId, c.StudentId, c.ClosureId, ct);
        if (x is null)
            return Result.Failure(EnrollmentClosureErrors.NotFound);
        var result = x.ReviewCheck(c.Type, c.Status, c.Detail);
        if (result.IsSuccess)
            await db.SaveChangesAsync(ct);
        return result;
    }

    public async Task<Result> CloseAsync(CloseEnrollmentCommand c, CancellationToken ct = default)
    {
        var x = await Find(c.OrganizationId, c.StudentId, c.ClosureId, ct);
        if (x is null)
            return Result.Failure(EnrollmentClosureErrors.NotFound);
        if (x.Status != EnrollmentClosureStatus.ReadyToClose)
            return Result.Failure(EnrollmentClosureErrors.PreconditionsNotResolved);
        var enrollment = await db.Enrollments.SingleOrDefaultAsync(
            e => e.Id == x.EnrollmentId && e.OrganizationId == c.OrganizationId,
            ct
        );
        if (enrollment is null)
            return Result.Failure(EnrollmentClosureErrors.ActiveEnrollmentNotFound);
        var board = await db
            .StudentStatusBoards.Include(b => b.Blocks)
            .Include(b => b.History)
            .SingleOrDefaultAsync(
                b => b.OrganizationId == c.OrganizationId && b.StudentId == c.StudentId,
                ct
            );
        if (board is null)
            return Result.Failure(EnrollmentClosureErrors.StatusBoardNotFound);
        var closed = enrollment.Close(c.ActorUserId, clock.UtcNow);
        if (closed.IsFailure)
            return closed;
        var block = board.ApplyBlock(
            "EnrollmentClosure",
            x.Reason.ToString(),
            "Students",
            StudentBlockingAction.Schedule
                | StudentBlockingAction.StartLesson
                | StudentBlockingAction.Sign
                | StudentBlockingAction.PresentExam
                | StudentBlockingAction.Transfer
                | StudentBlockingAction.Refund
                | StudentBlockingAction.PortalAccess,
            StudentBlockSeverity.Critical,
            "Controlled reopening or new enrollment",
            c.ActorUserId,
            clock.UtcNow
        );
        if (block.IsFailure)
            return Result.Failure(block.Error);
        var branchPortfolio = await db
            .StudentBranchPortfolios.Include(p => p.Assignments)
            .SingleOrDefaultAsync(
                p => p.OrganizationId == c.OrganizationId && p.StudentId == c.StudentId,
                ct
            );
        if (branchPortfolio is not null)
            foreach (
                var a in branchPortfolio
                    .Assignments.Where(a =>
                        a.Status
                            is DriveOS
                                    .Modules
                                    .Students
                                    .Domain
                                    .Branches
                                    .StudentBranchAssignmentStatus
                                    .Active
                                or DriveOS
                                    .Modules
                                    .Students
                                    .Domain
                                    .Branches
                                    .StudentBranchAssignmentStatus
                                    .Planned
                    )
                    .ToArray()
            )
            {
                var ended = branchPortfolio.End(
                    a.Id,
                    "Enrollment closure",
                    c.ActorUserId,
                    clock.UtcNow
                );
                if (ended.IsFailure)
                    return ended;
            }
        var instructorPortfolio = await db
            .StudentInstructorPortfolios.Include(p => p.Assignments)
            .Include(p => p.AccessGrants)
            .Include(p => p.History)
            .SingleOrDefaultAsync(
                p => p.OrganizationId == c.OrganizationId && p.StudentId == c.StudentId,
                ct
            );
        if (instructorPortfolio is not null)
            foreach (
                var a in instructorPortfolio
                    .Assignments.Where(a =>
                        a.Status
                            is DriveOS
                                    .Modules
                                    .Students
                                    .Domain
                                    .Instructors
                                    .StudentInstructorAssignmentStatus
                                    .Active
                                or DriveOS
                                    .Modules
                                    .Students
                                    .Domain
                                    .Instructors
                                    .StudentInstructorAssignmentStatus
                                    .Planned
                    )
                    .ToArray()
            )
            {
                var ended = instructorPortfolio.End(
                    a.Id,
                    "Enrollment closure",
                    c.ActorUserId,
                    clock.UtcNow
                );
                if (ended.IsFailure)
                    return ended;
            }
        var transfers = await db
            .ExternalTransferCases.Include(t => t.DataGrants)
            .Include(t => t.Audit)
            .Where(t => t.SourceOrganizationId == c.OrganizationId && t.StudentId == c.StudentId)
            .ToListAsync(ct);
        foreach (var transfer in transfers)
            transfer.RevokeActiveDataGrants(c.ActorUserId, clock.UtcNow);
        var result = x.Close(block.Value, c.ActorUserId, clock.UtcNow);
        if (result.IsSuccess)
            await db.SaveChangesAsync(ct);
        return result;
    }

    public async Task<Result> ArchiveAsync(ArchiveStudentCommand c, CancellationToken ct = default)
    {
        var x = await Find(c.OrganizationId, c.StudentId, c.ClosureId, ct);
        if (x is null)
            return Result.Failure(EnrollmentClosureErrors.NotFound);
        if (
            await db
                .Enrollments.AsNoTracking()
                .AnyAsync(
                    e =>
                        e.OrganizationId == c.OrganizationId
                        && e.StudentId == c.StudentId
                        && e.Id != x.EnrollmentId
                        && (
                            e.Status == EnrollmentStatus.Active
                            || e.Status == EnrollmentStatus.Suspended
                        ),
                    ct
                )
        )
            return Result.Failure(EnrollmentClosureErrors.InvalidTransition);
        var student = await db.Students.SingleOrDefaultAsync(
            s => s.OrganizationId == c.OrganizationId && s.Id == c.StudentId,
            ct
        );
        if (student is null)
            return Result.Failure(EnrollmentClosureErrors.InvalidOwner);
        var result = x.Archive(
            c.RetainUntil,
            c.RetentionLegalBasis,
            c.RetentionScope,
            c.ActorUserId,
            clock.UtcNow
        );
        if (result.IsSuccess)
        {
            student.Archive(c.ActorUserId, clock.UtcNow);
            await db.SaveChangesAsync(ct);
        }
        return result;
    }

    public async Task<Result> ReopenAsync(ReopenEnrollmentCommand c, CancellationToken ct = default)
    {
        var x = await Find(c.OrganizationId, c.StudentId, c.ClosureId, ct);
        if (x is null)
            return Result.Failure(EnrollmentClosureErrors.NotFound);
        if (!x.OperationalBlockId.HasValue)
            return Result.Failure(EnrollmentClosureErrors.InvalidTransition);
        if (
            await db
                .Enrollments.AsNoTracking()
                .AnyAsync(
                    e =>
                        e.OrganizationId == c.OrganizationId
                        && e.StudentId == c.StudentId
                        && e.Id != x.EnrollmentId
                        && (
                            e.Status == EnrollmentStatus.Active
                            || e.Status == EnrollmentStatus.Suspended
                        ),
                    ct
                )
        )
            return Result.Failure(EnrollmentClosureErrors.InvalidTransition);
        var enrollment = await db.Enrollments.SingleOrDefaultAsync(
            e => e.Id == x.EnrollmentId && e.OrganizationId == c.OrganizationId,
            ct
        );
        if (enrollment is null)
            return Result.Failure(EnrollmentClosureErrors.ActiveEnrollmentNotFound);
        var student = await db.Students.SingleOrDefaultAsync(
            s => s.OrganizationId == c.OrganizationId && s.Id == c.StudentId,
            ct
        );
        if (student is null)
            return Result.Failure(EnrollmentClosureErrors.InvalidOwner);
        var board = await db
            .StudentStatusBoards.Include(b => b.Blocks)
            .Include(b => b.History)
            .SingleOrDefaultAsync(
                b => b.OrganizationId == c.OrganizationId && b.StudentId == c.StudentId,
                ct
            );
        if (board is null)
            return Result.Failure(EnrollmentClosureErrors.StatusBoardNotFound);
        var reopened = x.Reopen(c.Justification, c.ActorUserId, clock.UtcNow);
        if (reopened.IsFailure)
            return reopened;
        var released = board.Release(
            x.OperationalBlockId.Value,
            StudentBlockResolutionType.HumanValidation,
            c.Justification,
            c.ActorUserId,
            clock.UtcNow
        );
        if (released.IsFailure)
            return released;
        bool suspensionActive = await db
            .EnrollmentSuspensions.AsNoTracking()
            .AnyAsync(
                s =>
                    s.EnrollmentId == x.EnrollmentId
                    && s.Status
                        == DriveOS
                            .Modules
                            .Students
                            .Domain
                            .Suspensions
                            .EnrollmentSuspensionStatus
                            .Active,
                ct
            );
        bool remainsSuspended =
            suspensionActive || x.PreviousEnrollmentStatus == EnrollmentStatus.Suspended;
        var enrollmentResult = remainsSuspended
            ? enrollment.ReopenAsSuspended(c.ActorUserId, clock.UtcNow)
            : enrollment.ReopenAsActive(c.ActorUserId, clock.UtcNow);
        if (enrollmentResult.IsSuccess)
        {
            if (remainsSuspended)
                student.RestoreAsSuspended(c.ActorUserId, clock.UtcNow);
            else
                student.RestoreAsActive(c.ActorUserId, clock.UtcNow);
            await db.SaveChangesAsync(ct);
        }
        return enrollmentResult;
    }

    private Task<EnrollmentClosureCase?> Find(
        DriveOS.SharedKernel.Identifiers.OrganizationId org,
        DriveOS.SharedKernel.Identifiers.PersonId student,
        Guid id,
        CancellationToken ct
    ) =>
        db
            .EnrollmentClosures.Include(x => x.Checks)
            .SingleOrDefaultAsync(
                x =>
                    x.Id == new DriveOS.SharedKernel.Identifiers.EnrollmentClosureCaseId(id)
                    && x.OrganizationId == org
                    && x.StudentId == student,
                ct
            );

    private static EnrollmentClosureResponse Map(EnrollmentClosureCase x) =>
        new(
            x.Id,
            x.EnrollmentId.Value,
            x.Reason,
            x.ClosureDate,
            x.ReasonDetail,
            x.Status,
            x.ClosedAtUtc,
            x.ArchivedAtUtc,
            x.RetainUntil,
            x.RetentionLegalBasis,
            x.RetentionScope,
            x.ReopenedAtUtc,
            x.ReopenJustification,
            x.Checks.OrderBy(c => c.Type)
                .Select(c => new EnrollmentClosureCheckItem(c.Type, c.Status, c.Detail))
                .ToArray()
        );
}
