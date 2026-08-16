using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Students.Application.Checklists;
using DriveOS.Modules.Students.Domain.Checklists;
using DriveOS.Modules.Students.Domain.Enrollments;
using DriveOS.Modules.Students.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Students.Infrastructure.Checklists;

internal sealed class EnrollmentChecklistService(StudentsDbContext db, IClock clock)
    : IEnrollmentChecklistService
{
    public async Task<EnrollmentChecklistResponse?> GetAsync(
        GetEnrollmentChecklistQuery q,
        CancellationToken ct = default
    )
    {
        var enrollment = await Enrollment(q.OrganizationId, q.StudentId, q.EnrollmentId, ct);
        if (enrollment is null)
            return null;
        var checklist = await Query(false)
            .SingleOrDefaultAsync(
                x =>
                    x.OrganizationId == q.OrganizationId
                    && x.StudentId == q.StudentId
                    && x.EnrollmentId == enrollment.Id,
                ct
            );
        return checklist is null ? Empty(q.StudentId, enrollment.Id) : Map(checklist);
    }

    public async Task<Result<int>> SynchronizeAsync(
        SynchronizeEnrollmentChecklistCommand x,
        CancellationToken ct = default
    )
    {
        var enrollment = await Enrollment(x.OrganizationId, x.StudentId, x.EnrollmentId, ct);
        if (enrollment is null)
            return Result.Failure<int>(EnrollmentChecklistApplicationErrors.EnrollmentNotFound);
        var rules = await db
            .EnrollmentChecklistRules.Where(r =>
                r.OrganizationId == x.OrganizationId
                && r.IsActive
                && (r.TrainingCode == "*" || r.TrainingCode == enrollment.TrainingCode)
            )
            .ToListAsync(ct);
        if (rules.Count == 0)
        {
            rules = Baseline(x.OrganizationId).ToList();
            db.EnrollmentChecklistRules.AddRange(rules);
        }
        var checklist = await GetOrCreate(x.OrganizationId, x.StudentId, enrollment.Id, ct);
        DateTimeOffset now = clock.UtcNow;
        foreach (var rule in rules)
            checklist.UpsertRule(
                rule.Id,
                rule.Code,
                rule.LabelKey,
                rule.Category,
                rule.IsBlocking,
                rule.TargetRoute,
                null,
                rule.DueInDays == 0 ? null : now.AddDays(rule.DueInDays),
                x.ActorUserId,
                now
            );
        await db.SaveChangesAsync(ct);
        return Result.Success(rules.Count);
    }

    public async Task<Result> ChangeStatusAsync(
        ChangeChecklistItemStatusCommand x,
        CancellationToken ct = default
    )
    {
        if (x.Status == ChecklistItemStatus.Waived && !x.CanApproveException)
            return Result.Failure(EnrollmentChecklistApplicationErrors.ExceptionApprovalForbidden);
        return await Change(
            x.OrganizationId,
            x.StudentId,
            x.EnrollmentId,
            c => c.ChangeStatus(x.ItemId, x.Status, x.Reason, x.ActorUserId, clock.UtcNow),
            ct
        );
    }

    public Task<Result> AssignAsync(AssignChecklistItemCommand x, CancellationToken ct = default) =>
        Change(
            x.OrganizationId,
            x.StudentId,
            x.EnrollmentId,
            c => c.Assign(x.ItemId, x.ResponsibleUserId, x.ActorUserId, clock.UtcNow),
            ct
        );

    public Task<Result> RemindAsync(RemindChecklistItemCommand x, CancellationToken ct = default) =>
        Change(
            x.OrganizationId,
            x.StudentId,
            x.EnrollmentId,
            c => c.Remind(x.ItemId, x.ActorUserId, clock.UtcNow),
            ct
        );

    public async Task<Result> ActivateAsync(
        ActivateEnrollmentCommand x,
        CancellationToken ct = default
    )
    {
        var enrollment = await Enrollment(x.OrganizationId, x.StudentId, x.EnrollmentId, ct, true);
        if (enrollment is null)
            return Result.Failure(EnrollmentChecklistApplicationErrors.EnrollmentNotFound);
        var checklist = await Query(true)
            .SingleOrDefaultAsync(
                c =>
                    c.OrganizationId == x.OrganizationId
                    && c.StudentId == x.StudentId
                    && c.EnrollmentId == x.EnrollmentId,
                ct
            );
        if (checklist is null)
            return Result.Failure(EnrollmentChecklistApplicationErrors.ChecklistNotFound);
        if (!checklist.CanActivate())
            return Result.Failure(EnrollmentChecklistErrors.BlockingItemsIncomplete);
        var result = enrollment.Activate(x.ActorUserId, clock.UtcNow);
        if (result.IsSuccess)
            await db.SaveChangesAsync(ct);
        return result;
    }

    public async Task<Result<Guid>> ConfigureRuleAsync(
        ConfigureChecklistRuleCommand x,
        CancellationToken ct = default
    )
    {
        EnrollmentChecklistRule rule;
        if (x.RuleId.HasValue)
        {
            var existing = await db.EnrollmentChecklistRules.SingleOrDefaultAsync(
                r => r.Id == x.RuleId && r.OrganizationId == x.OrganizationId,
                ct
            );
            if (existing is null)
                return Result.Failure<Guid>(EnrollmentChecklistApplicationErrors.RuleNotFound);
            rule = existing;
            rule.Update(
                x.LabelKey,
                x.Category,
                x.IsBlocking,
                x.TargetRoute,
                x.DueInDays,
                x.IsActive
            );
        }
        else
        {
            rule = EnrollmentChecklistRule.Create(
                x.OrganizationId,
                x.TrainingCode,
                x.Code,
                x.LabelKey,
                x.Category,
                x.IsBlocking,
                x.TargetRoute,
                x.DueInDays
            );
            db.EnrollmentChecklistRules.Add(rule);
        }
        await db.SaveChangesAsync(ct);
        return Result.Success(rule.Id);
    }

    private async Task<Result> Change(
        OrganizationId org,
        PersonId studentId,
        DraftEnrollmentId enrollmentId,
        Func<EnrollmentChecklist, Result> action,
        CancellationToken ct
    )
    {
        var c = await Query(true)
            .SingleOrDefaultAsync(
                x =>
                    x.OrganizationId == org
                    && x.StudentId == studentId
                    && x.EnrollmentId == enrollmentId,
                ct
            );
        if (c is null)
            return Result.Failure(EnrollmentChecklistApplicationErrors.ChecklistNotFound);
        var r = action(c);
        if (r.IsSuccess)
            await db.SaveChangesAsync(ct);
        return r;
    }

    private IQueryable<EnrollmentChecklist> Query(bool tracked) =>
        tracked
            ? db.EnrollmentChecklists.Include(x => x.Items)
            : db.EnrollmentChecklists.AsNoTracking().Include(x => x.Items);

    private async Task<EnrollmentChecklist> GetOrCreate(
        OrganizationId org,
        PersonId studentId,
        DraftEnrollmentId enrollmentId,
        CancellationToken ct
    )
    {
        var c = await Query(true)
            .SingleOrDefaultAsync(
                x =>
                    x.OrganizationId == org
                    && x.StudentId == studentId
                    && x.EnrollmentId == enrollmentId,
                ct
            );
        if (c is not null)
            return c;
        c = EnrollmentChecklist.Create(org, studentId, enrollmentId).Value;
        db.EnrollmentChecklists.Add(c);
        return c;
    }

    private async Task<Enrollment?> Enrollment(
        OrganizationId org,
        PersonId studentId,
        DraftEnrollmentId? id,
        CancellationToken ct,
        bool tracked = false
    )
    {
        IQueryable<Enrollment> q = tracked ? db.Enrollments : db.Enrollments.AsNoTracking();
        q = q.Where(e => e.OrganizationId == org && e.StudentId == studentId);
        return id.HasValue
            ? await q.SingleOrDefaultAsync(e => e.Id == id.Value, ct)
            : await q.OrderByDescending(e => e.CreatedAtUtc).FirstOrDefaultAsync(ct);
    }

    private static EnrollmentChecklistResponse Empty(
        PersonId studentId,
        DraftEnrollmentId enrollmentId
    ) => new(studentId.Value, enrollmentId.Value, false, 0, 0, []);

    private static EnrollmentChecklistResponse Map(EnrollmentChecklist c)
    {
        int total = c.Items.Count(x => x.IsBlocking);
        int done = c.Items.Count(x =>
            x.IsBlocking
            && (x.Status is ChecklistItemStatus.Completed or ChecklistItemStatus.Waived)
        );
        return new(
            c.StudentId.Value,
            c.EnrollmentId.Value,
            c.CanActivate(),
            done,
            total,
            c.Items.OrderBy(x => x.Category)
                .ThenBy(x => x.Code)
                .Select(x => new ChecklistItemResponse(
                    x.Id,
                    x.RuleId,
                    x.Code,
                    x.LabelKey,
                    x.Category,
                    x.IsBlocking,
                    x.TargetRoute,
                    x.Status,
                    x.ResponsibleUserId,
                    x.DueAtUtc,
                    x.DecisionReason,
                    x.ReminderCount,
                    x.LastReminderAtUtc
                ))
                .ToArray()
        );
    }

    private static IEnumerable<EnrollmentChecklistRule> Baseline(OrganizationId org)
    {
        yield return EnrollmentChecklistRule.Create(
            org,
            "*",
            "IDENTITY",
            "students.checklist.identity",
            ChecklistCategory.Identity,
            true,
            "identity",
            3
        );
        yield return EnrollmentChecklistRule.Create(
            org,
            "*",
            "INITIAL_ASSESSMENT",
            "students.checklist.initialAssessment",
            ChecklistCategory.Pedagogy,
            true,
            "pedagogy/assessment",
            7
        );
        yield return EnrollmentChecklistRule.Create(
            org,
            "*",
            "CONTRACT",
            "students.checklist.contract",
            ChecklistCategory.Contract,
            true,
            "contracts",
            7
        );
        yield return EnrollmentChecklistRule.Create(
            org,
            "*",
            "INITIAL_PAYMENT",
            "students.checklist.initialPayment",
            ChecklistCategory.Finance,
            true,
            "finance",
            7
        );
        yield return EnrollmentChecklistRule.Create(
            org,
            "*",
            "IDENTITY_DOCUMENT",
            "students.checklist.identityDocument",
            ChecklistCategory.Documents,
            true,
            "documents",
            5
        );
        yield return EnrollmentChecklistRule.Create(
            org,
            "*",
            "LEARNING_PATH",
            "students.checklist.learningPath",
            ChecklistCategory.Pedagogy,
            true,
            "pedagogy/path",
            10
        );
        yield return EnrollmentChecklistRule.Create(
            org,
            "*",
            "STUDENT_ACCOUNT",
            "students.checklist.studentAccount",
            ChecklistCategory.UserAccount,
            false,
            "account",
            3
        );
    }
}
