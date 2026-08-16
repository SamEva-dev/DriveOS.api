using System.Data;
using System.Data.Common;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Students.Application.Branches;
using DriveOS.Modules.Students.Domain.Branches;
using DriveOS.Modules.Students.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Students.Infrastructure.Branches;

internal sealed class StudentBranchManagementService(
    StudentsDbContext db,
    IClock clock,
    IStudentBranchVerifier verifier,
    IStudentBranchImpactAnalyzer impacts
) : IStudentBranchService
{
    public async Task<StudentBranchesResponse?> GetAsync(
        GetStudentBranchesQuery q,
        CancellationToken ct = default
    )
    {
        if (
            !await db
                .Students.AsNoTracking()
                .AnyAsync(x => x.OrganizationId == q.OrganizationId && x.Id == q.StudentId, ct)
        )
            return null;
        var board = await db
            .StudentBranchPortfolios.AsNoTracking()
            .Include(x => x.Assignments)
            .SingleOrDefaultAsync(
                x => x.OrganizationId == q.OrganizationId && x.StudentId == q.StudentId,
                ct
            );
        var items =
            board
                ?.Assignments.OrderBy(x => x.Type)
                .ThenByDescending(x => x.EffectiveFrom)
                .Select(Map)
                .ToArray()
            ?? [];
        var today = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);
        Guid? primary = items
            .Where(x =>
                x.Type == StudentBranchAssignmentType.Primary
                && x.EffectiveFrom <= today
                && (!x.EffectiveTo.HasValue || x.EffectiveTo.Value >= today)
                && x.Status
                    is not StudentBranchAssignmentStatus.Ended
                        and not StudentBranchAssignmentStatus.Cancelled
            )
            .OrderByDescending(x => x.EffectiveFrom)
            .FirstOrDefault()
            ?.BranchId;
        if (primary is null)
        {
            var enrollment = await db
                .Enrollments.AsNoTracking()
                .Where(x => x.OrganizationId == q.OrganizationId && x.StudentId == q.StudentId)
                .OrderByDescending(x => x.CreatedAtUtc)
                .FirstOrDefaultAsync(ct);
            primary = enrollment?.BranchId.Value;
        }
        return new(q.StudentId.Value, primary, items);
    }

    public async Task<Result<Guid>> AssignAsync(
        AssignStudentBranchCommand c,
        CancellationToken ct = default
    )
    {
        var checks = await verifier.VerifyAsync(c.OrganizationId, c.BranchId, ct);
        if (checks.Any(x => x.Status == BranchVerificationStatus.Failed))
            return Result.Failure<Guid>(StudentBranchErrors.BranchNotEligible);
        var board = await Find(c.OrganizationId, c.StudentId, ct);
        if (board is null)
        {
            if (
                !await db
                    .Students.AsNoTracking()
                    .AnyAsync(x => x.OrganizationId == c.OrganizationId && x.Id == c.StudentId, ct)
            )
                return Result.Failure<Guid>(StudentBranchApplicationErrors.StudentNotFound);
            var created = StudentBranchPortfolio.Create(c.OrganizationId, c.StudentId);
            if (created.IsFailure)
                return Result.Failure<Guid>(created.Error);
            board = created.Value;
            db.StudentBranchPortfolios.Add(board);
        }
        var r = board.Assign(
            c.BranchId,
            c.Type,
            c.ServicesAllowed,
            c.EffectiveFrom,
            c.EffectiveTo,
            c.Reason,
            c.ActorUserId,
            clock.UtcNow
        );
        if (r.IsSuccess)
            await db.SaveChangesAsync(ct);
        return r;
    }

    public async Task<Result<PrimaryBranchChangeAnalysisResponse>> AnalyzePrimaryChangeAsync(
        AnalyzePrimaryBranchChangeCommand c,
        CancellationToken ct = default
    )
    {
        var checks = await verifier.VerifyAsync(c.OrganizationId, c.TargetBranchId, ct);
        if (checks.Any(x => x.Status == BranchVerificationStatus.Failed))
            return Result.Failure<PrimaryBranchChangeAnalysisResponse>(
                StudentBranchErrors.BranchNotEligible
            );
        var board = await Find(c.OrganizationId, c.StudentId, ct);
        if (board is null)
        {
            if (
                !await db
                    .Students.AsNoTracking()
                    .AnyAsync(x => x.OrganizationId == c.OrganizationId && x.Id == c.StudentId, ct)
            )
                return Result.Failure<PrimaryBranchChangeAnalysisResponse>(
                    StudentBranchApplicationErrors.StudentNotFound
                );
            board = StudentBranchPortfolio.Create(c.OrganizationId, c.StudentId).Value;
            db.StudentBranchPortfolios.Add(board);
            var enrollment = await db
                .Enrollments.AsNoTracking()
                .Where(x => x.OrganizationId == c.OrganizationId && x.StudentId == c.StudentId)
                .OrderByDescending(x => x.CreatedAtUtc)
                .FirstOrDefaultAsync(ct);
            if (enrollment is not null)
                board.Assign(
                    enrollment.BranchId,
                    StudentBranchAssignmentType.Primary,
                    StudentBranchService.None,
                    DateOnly.FromDateTime(clock.UtcNow.UtcDateTime),
                    null,
                    "Enrollment branch",
                    c.ActorUserId,
                    clock.UtcNow
                );
        }
        var current = board
            .Assignments.SingleOrDefault(x =>
                x.Type == StudentBranchAssignmentType.Primary
                && x.Status
                    is StudentBranchAssignmentStatus.Active
                        or StudentBranchAssignmentStatus.Planned
            )
            ?.BranchId;
        var analyzed = await impacts.AnalyzeAsync(
            c.OrganizationId,
            c.StudentId,
            current,
            c.TargetBranchId,
            ct
        );
        var entity = board.AnalyzePrimaryChange(
            c.TargetBranchId,
            analyzed
                .Select(x => new BranchChangeImpact(
                    x.Type,
                    x.AffectedCount,
                    x.MessageKey,
                    x.RequiresAction
                ))
                .ToArray(),
            c.ActorUserId,
            clock.UtcNow
        );
        await db.SaveChangesAsync(ct);
        return Result.Success(
            new PrimaryBranchChangeAnalysisResponse(
                entity.Id,
                entity.CurrentBranchId?.Value,
                entity.TargetBranchId.Value,
                entity.ExpiresAtUtc,
                checks,
                analyzed
            )
        );
    }

    public async Task<Result> ChangePrimaryAsync(
        ChangePrimaryStudentBranchCommand c,
        CancellationToken ct = default
    )
    {
        var board = await Find(c.OrganizationId, c.StudentId, ct);
        if (board is null)
            return Result.Failure(StudentBranchApplicationErrors.StudentNotFound);
        var r = board.ChangePrimary(c.AnalysisId, c.Reason, c.ActorUserId, clock.UtcNow);
        if (r.IsSuccess)
            await db.SaveChangesAsync(ct);
        return r;
    }

    public async Task<Result> EndAsync(
        EndStudentBranchAssignmentCommand c,
        CancellationToken ct = default
    )
    {
        var board = await Find(c.OrganizationId, c.StudentId, ct);
        if (board is null)
            return Result.Failure(StudentBranchApplicationErrors.StudentNotFound);
        var r = board.End(c.AssignmentId, c.Reason, c.ActorUserId, clock.UtcNow);
        if (r.IsSuccess)
            await db.SaveChangesAsync(ct);
        return r;
    }

    private Task<StudentBranchPortfolio?> Find(
        OrganizationId org,
        PersonId student,
        CancellationToken ct
    ) =>
        db
            .StudentBranchPortfolios.Include(x => x.Assignments)
            .Include(x => x.Analyses)
                .ThenInclude(x => x.Impacts)
            .SingleOrDefaultAsync(x => x.OrganizationId == org && x.StudentId == student, ct);

    private static StudentBranchAssignmentItem Map(StudentBranchAssignment x) =>
        new(
            x.Id,
            x.BranchId.Value,
            x.Type,
            x.ServicesAllowed,
            x.EffectiveFrom,
            x.EffectiveTo,
            x.Reason,
            x.Status
        );
}

internal sealed class StudentBranchVerifier(StudentsDbContext db) : IStudentBranchVerifier
{
    public async Task<IReadOnlyList<BranchVerificationItem>> VerifyAsync(
        OrganizationId org,
        BranchId branch,
        CancellationToken ct = default
    )
    {
        DbConnection connection = db.Database.GetDbConnection();
        bool close = connection.State != ConnectionState.Open;
        if (close)
            await connection.OpenAsync(ct);
        try
        {
            await using DbCommand command = connection.CreateCommand();
            command.CommandText =
                "SELECT status FROM organization.branches WHERE id = @id AND organization_id = @organization LIMIT 1";
            var id = command.CreateParameter();
            id.ParameterName = "id";
            id.Value = branch.Value;
            command.Parameters.Add(id);
            var organization = command.CreateParameter();
            organization.ParameterName = "organization";
            organization.Value = org.Value;
            command.Parameters.Add(organization);
            string? status = await command.ExecuteScalarAsync(ct) as string;
            var active =
                status == "Active"
                    ? BranchVerificationStatus.Passed
                    : BranchVerificationStatus.Failed;
            return
            [
                new(
                    "BranchActive",
                    active,
                    status is null
                        ? "errors.students.branches.branchNotFound"
                        : "students.branches.verifications.branchActive"
                ),
                new(
                    "TrainingAvailable",
                    BranchVerificationStatus.NotEvaluated,
                    "students.branches.verifications.trainingPending"
                ),
                new(
                    "Capacity",
                    BranchVerificationStatus.NotEvaluated,
                    "students.branches.verifications.capacityPending"
                ),
                new(
                    "Territory",
                    BranchVerificationStatus.NotEvaluated,
                    "students.branches.verifications.territoryPending"
                ),
                new(
                    "Pricing",
                    BranchVerificationStatus.NotEvaluated,
                    "students.branches.verifications.pricingPending"
                ),
                new(
                    "SharedData",
                    BranchVerificationStatus.Warning,
                    "students.branches.verifications.sharedDataReview"
                ),
                new(
                    "ExistingSchedule",
                    BranchVerificationStatus.Warning,
                    "students.branches.verifications.scheduleReview"
                ),
            ];
        }
        finally
        {
            if (close)
                await connection.CloseAsync();
        }
    }
}

internal sealed class StudentBranchImpactAnalyzer : IStudentBranchImpactAnalyzer
{
    public Task<IReadOnlyList<BranchChangeImpactItem>> AnalyzeAsync(
        OrganizationId org,
        PersonId student,
        BranchId? current,
        BranchId target,
        CancellationToken ct = default
    ) =>
        Task.FromResult<IReadOnlyList<BranchChangeImpactItem>>([
            new(
                BranchImpactType.FutureSessions,
                0,
                "students.branches.impacts.futureSessionsReview",
                true
            ),
            new(
                BranchImpactType.ReferenceInstructor,
                0,
                "students.branches.impacts.referenceInstructorReview",
                true
            ),
            new(
                BranchImpactType.LocalPricing,
                0,
                "students.branches.impacts.localPricingReview",
                true
            ),
            new(
                BranchImpactType.MeetingPoint,
                0,
                "students.branches.impacts.meetingPointReview",
                true
            ),
            new(
                BranchImpactType.LocalDocumentsAndRules,
                0,
                "students.branches.impacts.localRulesReview",
                true
            ),
        ]);
}
