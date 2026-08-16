using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Students.Application.Administration;
using DriveOS.Modules.Students.Domain.Administration;
using DriveOS.Modules.Students.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Students.Infrastructure.Administration;

internal sealed class AdministrationService(StudentsDbContext db, IClock clock)
    : IAdministrationService
{
    public async Task<AdministrationResponse?> GetAsync(
        OrganizationId org,
        PersonId studentId,
        CancellationToken ct = default
    )
    {
        if (!await StudentExists(org, studentId, ct))
            return null;
        var c = await Query(false)
            .SingleOrDefaultAsync(x => x.OrganizationId == org && x.StudentId == studentId, ct);
        return c is null ? Empty(studentId) : Map(c);
    }

    public async Task<Result<Guid>> ConfigureAsync(
        ConfigureRequirementCommand x,
        CancellationToken ct = default
    )
    {
        var c = await GetOrCreate(x.OrganizationId, x.StudentId, ct);
        if (c is null)
            return Result.Failure<Guid>(AdministrationApplicationErrors.StudentNotFound);
        var r = c.UpsertRequirement(
            x.RequirementId,
            x.Code,
            x.LabelKey,
            x.IsBlocking,
            x.DueAtUtc,
            x.PolicySource,
            x.ActorUserId,
            clock.UtcNow
        );
        if (r.IsSuccess)
            await db.SaveChangesAsync(ct);
        return r;
    }

    public async Task<Result> DecideRequirementAsync(
        DecideRequirementCommand x,
        CancellationToken ct = default
    )
    {
        var c = await GetTracked(x.OrganizationId, x.StudentId, ct);
        if (c is null)
            return Result.Failure(AdministrationApplicationErrors.StudentNotFound);
        var r = c.DecideRequirement(
            x.RequirementId,
            x.Status,
            x.Reason,
            x.ActorUserId,
            clock.UtcNow
        );
        if (r.IsSuccess)
            await db.SaveChangesAsync(ct);
        return r;
    }

    public async Task<Result<Guid>> AddBlockAsync(
        AddAdministrativeBlockCommand x,
        CancellationToken ct = default
    )
    {
        var c = await GetOrCreate(x.OrganizationId, x.StudentId, ct);
        if (c is null)
            return Result.Failure<Guid>(AdministrationApplicationErrors.StudentNotFound);
        var r = c.AddBlock(x.Code, x.Reason, x.ActorUserId, clock.UtcNow);
        if (r.IsSuccess)
            await db.SaveChangesAsync(ct);
        return r;
    }

    public async Task<Result> ReleaseBlockAsync(
        ReleaseAdministrativeBlockCommand x,
        CancellationToken ct = default
    )
    {
        var c = await GetTracked(x.OrganizationId, x.StudentId, ct);
        if (c is null)
            return Result.Failure(AdministrationApplicationErrors.StudentNotFound);
        var r = c.ReleaseBlock(x.BlockId, x.Reason, x.ActorUserId, clock.UtcNow);
        if (r.IsSuccess)
            await db.SaveChangesAsync(ct);
        return r;
    }

    public async Task<Result<Guid>> RequestExceptionAsync(
        RequestComplianceExceptionCommand x,
        CancellationToken ct = default
    )
    {
        var c = await GetTracked(x.OrganizationId, x.StudentId, ct);
        if (c is null)
            return Result.Failure<Guid>(AdministrationApplicationErrors.StudentNotFound);
        var r = c.RequestException(x.RequirementId, x.Reason, x.ActorUserId, clock.UtcNow);
        if (r.IsSuccess)
            await db.SaveChangesAsync(ct);
        return r;
    }

    public async Task<Result> DecideExceptionAsync(
        DecideComplianceExceptionCommand x,
        CancellationToken ct = default
    )
    {
        var c = await GetTracked(x.OrganizationId, x.StudentId, ct);
        if (c is null)
            return Result.Failure(AdministrationApplicationErrors.StudentNotFound);
        var r = c.DecideException(x.ExceptionId, x.Approve, x.Reason, x.ActorUserId, clock.UtcNow);
        if (r.IsSuccess)
            await db.SaveChangesAsync(ct);
        return r;
    }

    public async Task<Result<int>> SynchronizeRequirementsAsync(
        SynchronizeAdministrativeRequirementsCommand x,
        CancellationToken ct = default
    )
    {
        var student = await db
            .Students.AsNoTracking()
            .SingleOrDefaultAsync(
                s => s.OrganizationId == x.OrganizationId && s.Id == x.StudentId,
                ct
            );
        if (student is null)
            return Result.Failure<int>(AdministrationApplicationErrors.StudentNotFound);
        var enrollment = await db
            .Enrollments.AsNoTracking()
            .Where(e => e.OrganizationId == x.OrganizationId && e.StudentId == x.StudentId)
            .OrderByDescending(e => e.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);
        var c = await GetOrCreate(x.OrganizationId, x.StudentId, ct);
        string country = enrollment?.RegulatoryCountryCode ?? student.CountryCode ?? "FR";
        string training = enrollment?.TrainingCode ?? "GENERAL";
        string policy = $"{country}-{training}";
        int count = 0;
        foreach (var rule in BaselineRules(student.BirthDate))
        {
            var r = c!.UpsertRequirement(
                null,
                rule.Code,
                rule.LabelKey,
                rule.Blocking,
                null,
                policy,
                x.ActorUserId,
                clock.UtcNow
            );
            if (r.IsFailure)
                return Result.Failure<int>(r.Error);
            count++;
        }
        await db.SaveChangesAsync(ct);
        return Result.Success(count);
    }

    private IQueryable<AdministrativeCase> Query(bool tracked) =>
        tracked
            ? Includes(db.AdministrativeCases)
            : Includes(db.AdministrativeCases.AsNoTracking());

    private static IQueryable<AdministrativeCase> Includes(IQueryable<AdministrativeCase> q) =>
        q.Include(x => x.Requirements)
            .Include(x => x.Blocks)
            .Include(x => x.Exceptions)
            .Include(x => x.History);

    private Task<AdministrativeCase?> GetTracked(
        OrganizationId o,
        PersonId s,
        CancellationToken ct
    ) => Query(true).SingleOrDefaultAsync(x => x.OrganizationId == o && x.StudentId == s, ct);

    private async Task<AdministrativeCase?> GetOrCreate(
        OrganizationId o,
        PersonId s,
        CancellationToken ct
    )
    {
        var c = await GetTracked(o, s, ct);
        if (c is not null)
            return c;
        if (!await StudentExists(o, s, ct))
            return null;
        var r = AdministrativeCase.Create(o, s);
        db.AdministrativeCases.Add(r.Value);
        return r.Value;
    }

    private Task<bool> StudentExists(OrganizationId o, PersonId s, CancellationToken ct) =>
        db.Students.AsNoTracking().AnyAsync(x => x.OrganizationId == o && x.Id == s, ct);

    private static AdministrationResponse Empty(PersonId s) =>
        new(s.Value, AdministrativeStatus.ToComplete, 0, 0, [], [], [], []);

    private static AdministrationResponse Map(AdministrativeCase c) =>
        new(
            c.StudentId.Value,
            c.Status,
            c.Requirements.Count(x =>
                x.Status
                    is AdministrativeRequirementStatus.Validated
                        or AdministrativeRequirementStatus.Waived
            ),
            c.Requirements.Count,
            c.Requirements.OrderBy(x => x.Code)
                .Select(x => new RequirementItem(
                    x.Id,
                    x.Code,
                    x.LabelKey,
                    x.IsBlocking,
                    x.Status,
                    x.DueAtUtc,
                    x.PolicySource,
                    x.DecisionReason
                ))
                .ToArray(),
            c.Blocks.Where(x => x.ReleasedAtUtc == null)
                .Select(x => new BlockItem(x.Id, x.Code, x.Reason, x.AppliedAtUtc))
                .ToArray(),
            c.Exceptions.OrderByDescending(x => x.RequestedAtUtc)
                .Select(x => new ExceptionItem(
                    x.Id,
                    x.RequirementId,
                    x.RequestReason,
                    x.Status,
                    x.DecisionReason,
                    x.RequestedAtUtc
                ))
                .ToArray(),
            c.History.OrderByDescending(x => x.OccurredAtUtc)
                .Take(100)
                .Select(x => new AdministrationHistoryItem(x.Action, x.SubjectId, x.OccurredAtUtc))
                .ToArray()
        );

    private static IEnumerable<(string Code, string LabelKey, bool Blocking)> BaselineRules(
        DateOnly? birthDate
    )
    {
        yield return ("IDENTITY_PROOF", "students.administration.requirements.identityProof", true);
        yield return (
            "REQUIRED_CONSENTS",
            "students.administration.requirements.requiredConsents",
            true
        );
        if (
            birthDate.HasValue
            && birthDate.Value > AddYears(DateOnly.FromDateTime(DateTime.UtcNow), -18)
        )
            yield return (
                "GUARDIAN_AUTHORIZATION",
                "students.administration.requirements.guardianAuthorization",
                true
            );
    }

    private static DateOnly AddYears(DateOnly value, int years) => value.AddYears(years);
}
