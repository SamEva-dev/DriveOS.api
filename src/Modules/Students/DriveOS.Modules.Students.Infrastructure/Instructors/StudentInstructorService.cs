using System.Data;
using System.Data.Common;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Students.Application.Instructors;
using DriveOS.Modules.Students.Domain.Branches;
using DriveOS.Modules.Students.Domain.Instructors;
using DriveOS.Modules.Students.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Students.Infrastructure.Instructors;

internal sealed class StudentInstructorManagementService(
    StudentsDbContext db,
    IClock clock,
    IInstructorEligibilityGateway eligibility
) : IStudentInstructorService
{
    public async Task<StudentInstructorsResponse?> GetAsync(
        GetStudentInstructorsQuery q,
        CancellationToken ct = default
    )
    {
        if (
            !await db
                .Students.AsNoTracking()
                .AnyAsync(x => x.OrganizationId == q.OrganizationId && x.Id == q.StudentId, ct)
        )
            return null;
        var portfolio = await db
            .StudentInstructorPortfolios.AsNoTracking()
            .Include(x => x.Assignments)
            .Include(x => x.History)
            .SingleOrDefaultAsync(
                x => x.OrganizationId == q.OrganizationId && x.StudentId == q.StudentId,
                ct
            );
        var today = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);
        var items =
            portfolio
                ?.Assignments.OrderBy(x => x.Type)
                .ThenByDescending(x => x.EffectiveFrom)
                .Select(x => Map(x, today))
                .ToArray()
            ?? [];
        var history =
            portfolio
                ?.History.OrderByDescending(x => x.OccurredAtUtc)
                .Select(x => new StudentInstructorHistoryItem(
                    x.Id,
                    x.AssignmentId,
                    x.Action,
                    x.Reason,
                    x.ActorUserId.Value,
                    x.OccurredAtUtc
                ))
                .ToArray()
            ?? [];
        var primary = items
            .FirstOrDefault(x =>
                x.Type == StudentInstructorAssignmentType.PrimaryInstructor
                && (
                    x.Status
                    is StudentInstructorAssignmentStatus.Active
                        or StudentInstructorAssignmentStatus.Planned
                )
            )
            ?.InstructorId;
        return new(q.StudentId.Value, primary, items, history);
    }

    public async Task<IReadOnlyList<InstructorSuggestionItem>> GetSuggestionsAsync(
        GetInstructorSuggestionsQuery q,
        CancellationToken ct = default
    ) =>
        await eligibility.SuggestAsync(
            q.OrganizationId,
            q.BranchId ?? await ResolveBranch(q.OrganizationId, q.StudentId, ct),
            q.TrainingCategory,
            ct
        );

    public async Task<Result<Guid>> AssignAsync(
        AssignStudentInstructorCommand c,
        CancellationToken ct = default
    )
    {
        var branch = await ResolveBranch(c.OrganizationId, c.StudentId, ct);
        var check = await eligibility.VerifyAsync(
            c.OrganizationId,
            c.InstructorId,
            branch,
            c.TrainingCategory,
            ct
        );
        if (!check.IsEligible)
            return Result.Failure<Guid>(StudentInstructorErrors.InstructorNotEligible);
        var portfolio = await Find(c.OrganizationId, c.StudentId, ct);
        if (portfolio is null)
        {
            if (!await StudentExists(c.OrganizationId, c.StudentId, ct))
                return Result.Failure<Guid>(StudentInstructorApplicationErrors.StudentNotFound);
            var created = StudentInstructorPortfolio.Create(c.OrganizationId, c.StudentId);
            if (created.IsFailure)
                return Result.Failure<Guid>(created.Error);
            portfolio = created.Value;
            db.StudentInstructorPortfolios.Add(portfolio);
        }
        var result = portfolio.Assign(
            c.InstructorId,
            c.Type,
            c.EffectiveFrom,
            c.EffectiveTo,
            c.TrainingCategory,
            c.MaximumScope,
            c.Reason,
            c.ActorUserId,
            clock.UtcNow
        );
        if (result.IsSuccess)
            await db.SaveChangesAsync(ct);
        return result;
    }

    public async Task<Result> ReplacePrimaryAsync(
        ReplacePrimaryInstructorCommand c,
        CancellationToken ct = default
    )
    {
        var branch = await ResolveBranch(c.OrganizationId, c.StudentId, ct);
        var check = await eligibility.VerifyAsync(
            c.OrganizationId,
            c.InstructorId,
            branch,
            c.TrainingCategory,
            ct
        );
        if (!check.IsEligible)
            return Result.Failure(StudentInstructorErrors.InstructorNotEligible);
        var portfolio = await Find(c.OrganizationId, c.StudentId, ct);
        if (portfolio is null)
            return Result.Failure(StudentInstructorApplicationErrors.StudentNotFound);
        var result = portfolio.ReplacePrimary(
            c.InstructorId,
            c.EffectiveFrom,
            c.EffectiveTo,
            c.TrainingCategory,
            c.MaximumScope,
            c.Reason,
            c.ActorUserId,
            clock.UtcNow
        );
        if (result.IsSuccess)
            await db.SaveChangesAsync(ct);
        return result;
    }

    public async Task<Result> EndAsync(
        EndStudentInstructorAssignmentCommand c,
        CancellationToken ct = default
    )
    {
        var portfolio = await Find(c.OrganizationId, c.StudentId, ct);
        if (portfolio is null)
            return Result.Failure(StudentInstructorApplicationErrors.StudentNotFound);
        var result = portfolio.End(c.AssignmentId, c.Reason, c.ActorUserId, clock.UtcNow);
        if (result.IsSuccess)
            await db.SaveChangesAsync(ct);
        return result;
    }

    private Task<bool> StudentExists(OrganizationId org, PersonId student, CancellationToken ct) =>
        db.Students.AsNoTracking().AnyAsync(x => x.OrganizationId == org && x.Id == student, ct);

    private Task<StudentInstructorPortfolio?> Find(
        OrganizationId org,
        PersonId student,
        CancellationToken ct
    ) =>
        db
            .StudentInstructorPortfolios.Include(x => x.Assignments)
            .Include(x => x.AccessGrants)
            .Include(x => x.History)
            .SingleOrDefaultAsync(x => x.OrganizationId == org && x.StudentId == student, ct);

    private async Task<BranchId?> ResolveBranch(
        OrganizationId org,
        PersonId student,
        CancellationToken ct
    )
    {
        var assignment = await db
            .StudentBranchPortfolios.AsNoTracking()
            .Where(x => x.OrganizationId == org && x.StudentId == student)
            .SelectMany(x => x.Assignments)
            .Where(x =>
                x.Type == StudentBranchAssignmentType.Primary
                && x.Status != StudentBranchAssignmentStatus.Ended
            )
            .OrderByDescending(x => x.EffectiveFrom)
            .FirstOrDefaultAsync(ct);
        if (assignment is not null)
            return assignment.BranchId;
        var enrollment = await db
            .Enrollments.AsNoTracking()
            .Where(x => x.OrganizationId == org && x.StudentId == student)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);
        return enrollment?.BranchId;
    }

    private static StudentInstructorAssignmentItem Map(
        StudentInstructorAssignment x,
        DateOnly today
    )
    {
        var status = x.Status;
        if (
            (
                status
                is StudentInstructorAssignmentStatus.Active
                    or StudentInstructorAssignmentStatus.Planned
            )
            && x.EffectiveTo.HasValue
            && x.EffectiveTo.Value < today
        )
            status = StudentInstructorAssignmentStatus.Expired;
        return new(
            x.Id,
            x.InstructorId.Value,
            x.Type,
            x.EffectiveFrom,
            x.EffectiveTo,
            x.TrainingCategory,
            x.MaximumScope,
            x.Reason,
            status
        );
    }
}

internal sealed class InstructorEligibilityGateway(StudentsDbContext db)
    : IInstructorEligibilityGateway
{
    public async Task<InstructorEligibility> VerifyAsync(
        OrganizationId org,
        UserId instructor,
        BranchId? branch,
        string category,
        CancellationToken ct = default
    )
    {
        var matches = await Read(org, branch, instructor, ct);
        var warnings = new List<string>();
        if (string.IsNullOrWhiteSpace(category))
            warnings.Add("errors.students.instructors.trainingCategory.required");
        if (branch is null)
            warnings.Add("warnings.students.instructors.branch.notResolved");
        if (matches.Count == 0)
            warnings.Add("errors.students.instructors.instructor.notAssignedToBranch");
        return new(matches.Count > 0 && !string.IsNullOrWhiteSpace(category), warnings);
    }

    public async Task<IReadOnlyList<InstructorSuggestionItem>> SuggestAsync(
        OrganizationId org,
        BranchId? branch,
        string category,
        CancellationToken ct = default
    )
    {
        var rows = await Read(org, branch, null, ct);
        return rows.Select(x => new InstructorSuggestionItem(
                x.UserId,
                x.BranchId,
                null,
                category,
                true,
                InstructorMetricStatus.NotEvaluated,
                null,
                null,
                null,
                false,
                false,
                ["warnings.students.instructors.metrics.notAvailable"]
            ))
            .ToArray();
    }

    private async Task<List<(Guid UserId, Guid BranchId)>> Read(
        OrganizationId org,
        BranchId? branch,
        UserId? user,
        CancellationToken ct
    )
    {
        DbConnection connection = db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT user_id, branch_id FROM organization.branch_user_assignments "
            + "WHERE organization_id = @organization_id "
            + "AND role = 'Instructor' AND status = 'Active' "
            + "AND (@branch_id IS NULL OR branch_id = @branch_id) "
            + "AND (@user_id IS NULL OR user_id = @user_id)";
        Add(command, "organization_id", org.Value);
        Add(command, "branch_id", branch?.Value);
        Add(command, "user_id", user?.Value);
        var result = new List<(Guid, Guid)>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            result.Add((reader.GetGuid(0), reader.GetGuid(1)));
        return result;
    }

    private static void Add(DbCommand command, string name, object? value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.DbType = DbType.Guid;
        parameter.Value = value ?? DBNull.Value;
        command.Parameters.Add(parameter);
    }
}
