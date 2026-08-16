using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Students.Application.Statuses;
using DriveOS.Modules.Students.Domain.Administration;
using DriveOS.Modules.Students.Domain.Enrollments;
using DriveOS.Modules.Students.Domain.Statuses;
using DriveOS.Modules.Students.Domain.Students;
using DriveOS.Modules.Students.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Students.Infrastructure.Statuses;

internal sealed class StudentStatusService(StudentsDbContext db, IClock clock)
    : IStudentStatusService
{
    public async Task<StudentStatusesResponse?> GetAsync(
        OrganizationId org,
        PersonId studentId,
        CancellationToken ct = default
    )
    {
        var student = await db
            .Students.AsNoTracking()
            .SingleOrDefaultAsync(x => x.OrganizationId == org && x.Id == studentId, ct);
        if (student is null)
            return null;
        var enrollment = await db
            .Enrollments.AsNoTracking()
            .Where(x => x.OrganizationId == org && x.StudentId == studentId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => (EnrollmentStatus?)x.Status)
            .FirstOrDefaultAsync(ct);
        var administrative =
            await db
                .AdministrativeCases.AsNoTracking()
                .Where(x => x.OrganizationId == org && x.StudentId == studentId)
                .Select(x => (AdministrativeStatus?)x.Status)
                .SingleOrDefaultAsync(ct)
            ?? AdministrativeStatus.ToComplete;
        var board = await db
            .StudentStatusBoards.AsNoTracking()
            .Include(x => x.Blocks)
            .SingleOrDefaultAsync(x => x.OrganizationId == org && x.StudentId == studentId, ct);
        return Map(
            student.Id.Value,
            student.Status,
            enrollment,
            administrative,
            board,
            clock.UtcNow
        );
    }

    public async Task<Result<Guid>> ApplyBlockAsync(
        ApplyStudentBlockCommand x,
        CancellationToken ct = default
    )
    {
        var board = await FindTracked(x.OrganizationId, x.StudentId, ct);
        if (board is null)
        {
            if (!await StudentExists(x.OrganizationId, x.StudentId, ct))
                return Result.Failure<Guid>(StudentStatusApplicationErrors.StudentNotFound);
            var created = StudentStatusBoard.Create(x.OrganizationId, x.StudentId);
            if (created.IsFailure)
                return Result.Failure<Guid>(created.Error);
            board = created.Value;
            db.StudentStatusBoards.Add(board);
        }
        var result = board.ApplyBlock(
            x.BlockType,
            x.Reason,
            x.SourceDomain,
            x.BlockingActions,
            x.Severity,
            x.ExpectedResolution,
            x.ActorUserId,
            clock.UtcNow
        );
        if (result.IsSuccess)
            await db.SaveChangesAsync(ct);
        return result;
    }

    public async Task<Result> ReleaseBlockAsync(
        ReleaseStudentBlockCommand x,
        CancellationToken ct = default
    )
    {
        var board = await FindTracked(x.OrganizationId, x.StudentId, ct);
        if (board is null)
            return Result.Failure(StudentStatusErrors.BlockNotFound);
        var result = board.Release(
            x.BlockId,
            x.ResolutionType,
            x.Reason,
            x.ActorUserId,
            clock.UtcNow
        );
        if (result.IsSuccess)
            await db.SaveChangesAsync(ct);
        return result;
    }

    public async Task<Result> OverrideBlockAsync(
        OverrideStudentBlockCommand x,
        CancellationToken ct = default
    )
    {
        var board = await FindTracked(x.OrganizationId, x.StudentId, ct);
        if (board is null)
            return Result.Failure(StudentStatusErrors.BlockNotFound);
        var result = board.Override(x.BlockId, x.Reason, x.UntilUtc, x.ActorUserId, clock.UtcNow);
        if (result.IsSuccess)
            await db.SaveChangesAsync(ct);
        return result;
    }

    private Task<bool> StudentExists(OrganizationId org, PersonId id, CancellationToken ct) =>
        db.Students.AsNoTracking().AnyAsync(x => x.OrganizationId == org && x.Id == id, ct);

    private Task<StudentStatusBoard?> FindTracked(
        OrganizationId org,
        PersonId id,
        CancellationToken ct
    ) =>
        db
            .StudentStatusBoards.Include(x => x.Blocks)
            .Include(x => x.History)
            .SingleOrDefaultAsync(x => x.OrganizationId == org && x.StudentId == id, ct);

    private static StudentStatusesResponse Map(
        Guid id,
        StudentStatus profile,
        EnrollmentStatus? enrollment,
        AdministrativeStatus administrative,
        StudentStatusBoard? board,
        DateTimeOffset now
    )
    {
        var blocks =
            board
                ?.Blocks.OrderByDescending(x => x.AppliedAtUtc)
                .Select(x => MapBlock(x, now))
                .ToArray()
            ?? [];
        var blocked = blocks
            .Where(x => x.Status == StudentBlockStatus.Active)
            .Aggregate(StudentBlockingAction.None, (value, x) => value | x.BlockingActions);
        return new(
            id,
            profile,
            enrollment,
            administrative,
            board?.FinancialStatus ?? FinancialStatus.Unknown,
            board?.PedagogicalStatus ?? PedagogicalStatus.NotStarted,
            board?.SchedulingStatus ?? SchedulingStatus.Allowed,
            board?.ExamStatus ?? ExamStatus.NotReady,
            board?.PortalAccessStatus ?? PortalAccessStatus.NotInvited,
            blocked,
            blocks
        );
    }

    private static StudentBlockItem MapBlock(StudentOperationalBlock x, DateTimeOffset now)
    {
        var effective =
            x.Status == StudentBlockStatus.Overridden && x.OverrideUntilUtc <= now
                ? StudentBlockStatus.Active
                : x.Status;
        return new(
            x.Id,
            x.BlockType,
            x.Reason,
            x.SourceDomain,
            x.BlockingActions,
            x.Severity,
            x.AppliedAtUtc,
            x.AppliedByUserId.Value,
            x.ExpectedResolution,
            effective,
            x.ResolutionType,
            x.ResolutionReason,
            x.ResolvedAtUtc,
            x.OverrideUntilUtc,
            x.OverrideReason
        );
    }
}
