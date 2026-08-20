using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Students.Application.Checklists;
using DriveOS.Modules.Students.Domain.Checklists;
using DriveOS.Modules.Students.Domain.Documents;
using DriveOS.Modules.Students.Domain.Enrollments;
using DriveOS.Modules.Students.Domain.Students;
using DriveOS.Modules.Students.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Students.Infrastructure.Checklists;

internal sealed class EnrollmentChecklistService(
    StudentsDbContext db,
    IClock clock,
    IEnrollmentPrerequisiteSnapshotProvider prerequisiteSnapshotProvider)
    : IEnrollmentChecklistService
{
    private static readonly HashSet<string> DerivedCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "IDENTITY",
        "IDENTITY_DOCUMENT",
        "CONTRACT",
        "INITIAL_PAYMENT",
        "INITIAL_ASSESSMENT",
        "LEARNING_PATH",
    };

    public async Task<EnrollmentChecklistResponse?> GetAsync(
        GetEnrollmentChecklistQuery q,
        CancellationToken ct = default)
    {
        Enrollment? enrollment = await Enrollment(q.OrganizationId, q.StudentId, q.EnrollmentId, ct);
        if (enrollment is null)
            return null;

        EnrollmentChecklist? checklist = await Query(false)
            .SingleOrDefaultAsync(
                x => x.OrganizationId == q.OrganizationId
                     && x.StudentId == q.StudentId
                     && x.EnrollmentId == enrollment.Id,
                ct);

        if (checklist is null)
            return Empty(q.StudentId, enrollment.Id);

        IReadOnlyDictionary<string, PrerequisiteEvaluation> derived =
            await EvaluateDerivedAsync(q.OrganizationId, q.StudentId, enrollment, ct);

        return Map(checklist, derived);
    }

    public async Task<Result<int>> SynchronizeAsync(
        SynchronizeEnrollmentChecklistCommand x,
        CancellationToken ct = default)
    {
        Enrollment? enrollment = await Enrollment(x.OrganizationId, x.StudentId, x.EnrollmentId, ct);
        if (enrollment is null)
            return Result.Failure<int>(EnrollmentChecklistApplicationErrors.EnrollmentNotFound);

        List<EnrollmentChecklistRule> rules = await db.EnrollmentChecklistRules
            .Where(r =>
                r.OrganizationId == x.OrganizationId
                && r.IsActive
                && (r.TrainingCode == "*" || r.TrainingCode == enrollment.TrainingCode))
            .ToListAsync(ct);

        if (rules.Count == 0)
        {
            rules = Baseline(x.OrganizationId).ToList();
            db.EnrollmentChecklistRules.AddRange(rules);
        }

        EnrollmentChecklist checklist = await GetOrCreate(
            x.OrganizationId,
            x.StudentId,
            enrollment.Id,
            ct);

        DateTimeOffset now = clock.UtcNow;
        foreach (EnrollmentChecklistRule rule in rules)
        {
            checklist.UpsertRule(
                rule.Id,
                rule.Code,
                rule.LabelKey,
                rule.Category,
                rule.IsBlocking,
                NormalizeTargetRoute(rule.Code, rule.TargetRoute),
                null,
                rule.DueInDays == 0 ? null : now.AddDays(rule.DueInDays),
                x.ActorUserId,
                now);
        }

        IReadOnlyDictionary<string, PrerequisiteEvaluation> derived =
            await EvaluateDerivedAsync(x.OrganizationId, x.StudentId, enrollment, ct);
        ApplyDerivedStatuses(checklist, derived, x.ActorUserId, now);

        await db.SaveChangesAsync(ct);
        return Result.Success(rules.Count);
    }

    public async Task<Result> ChangeStatusAsync(
        ChangeChecklistItemStatusCommand x,
        CancellationToken ct = default)
    {
        if (x.Status == ChecklistItemStatus.Waived && !x.CanApproveException)
            return Result.Failure(EnrollmentChecklistApplicationErrors.ExceptionApprovalForbidden);

        EnrollmentChecklist? checklist = await Query(true)
            .SingleOrDefaultAsync(
                c => c.OrganizationId == x.OrganizationId
                     && c.StudentId == x.StudentId
                     && c.EnrollmentId == x.EnrollmentId,
                ct);
        if (checklist is null)
            return Result.Failure(EnrollmentChecklistApplicationErrors.ChecklistNotFound);

        EnrollmentChecklistItem? item = checklist.Items.SingleOrDefault(i => i.Id == x.ItemId);
        if (item is null)
            return Result.Failure(EnrollmentChecklistErrors.ItemNotFound);

        if (DerivedCodes.Contains(item.Code) && x.Status != ChecklistItemStatus.Waived)
            return Result.Failure(EnrollmentChecklistApplicationErrors.DerivedStatusManualChangeForbidden);

        Result result = checklist.ChangeStatus(
            x.ItemId,
            x.Status,
            x.Reason,
            x.ActorUserId,
            clock.UtcNow);
        if (result.IsSuccess)
            await db.SaveChangesAsync(ct);
        return result;
    }

    public Task<Result> AssignAsync(AssignChecklistItemCommand x, CancellationToken ct = default) =>
        Change(
            x.OrganizationId,
            x.StudentId,
            x.EnrollmentId,
            c => c.Assign(x.ItemId, x.ResponsibleUserId, x.ActorUserId, clock.UtcNow),
            ct);

    public Task<Result> RemindAsync(RemindChecklistItemCommand x, CancellationToken ct = default) =>
        Change(
            x.OrganizationId,
            x.StudentId,
            x.EnrollmentId,
            c => c.Remind(x.ItemId, x.ActorUserId, clock.UtcNow),
            ct);

    public async Task<Result> ActivateAsync(
        ActivateEnrollmentCommand x,
        CancellationToken ct = default)
    {
        Enrollment? enrollment = await Enrollment(
            x.OrganizationId,
            x.StudentId,
            x.EnrollmentId,
            ct,
            true);
        if (enrollment is null)
            return Result.Failure(EnrollmentChecklistApplicationErrors.EnrollmentNotFound);

        EnrollmentChecklist? checklist = await Query(true)
            .SingleOrDefaultAsync(
                c => c.OrganizationId == x.OrganizationId
                     && c.StudentId == x.StudentId
                     && c.EnrollmentId == x.EnrollmentId,
                ct);
        if (checklist is null)
            return Result.Failure(EnrollmentChecklistApplicationErrors.ChecklistNotFound);

        IReadOnlyDictionary<string, PrerequisiteEvaluation> derived =
            await EvaluateDerivedAsync(x.OrganizationId, x.StudentId, enrollment, ct);
        ApplyDerivedStatuses(checklist, derived, x.ActorUserId, clock.UtcNow);

        if (!checklist.CanActivate())
            return Result.Failure(EnrollmentChecklistErrors.BlockingItemsIncomplete);

        Result result = enrollment.Activate(x.ActorUserId, clock.UtcNow);
        if (result.IsSuccess)
            await db.SaveChangesAsync(ct);
        return result;
    }

    public async Task<Result<Guid>> ConfigureRuleAsync(
        ConfigureChecklistRuleCommand x,
        CancellationToken ct = default)
    {
        EnrollmentChecklistRule rule;
        if (x.RuleId.HasValue)
        {
            EnrollmentChecklistRule? existing = await db.EnrollmentChecklistRules
                .SingleOrDefaultAsync(
                    r => r.Id == x.RuleId && r.OrganizationId == x.OrganizationId,
                    ct);
            if (existing is null)
                return Result.Failure<Guid>(EnrollmentChecklistApplicationErrors.RuleNotFound);
            rule = existing;
            rule.Update(
                x.LabelKey,
                x.Category,
                x.IsBlocking,
                x.TargetRoute,
                x.DueInDays,
                x.IsActive);
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
                x.DueInDays);
            db.EnrollmentChecklistRules.Add(rule);
        }

        await db.SaveChangesAsync(ct);
        return Result.Success(rule.Id);
    }

    private async Task<IReadOnlyDictionary<string, PrerequisiteEvaluation>> EvaluateDerivedAsync(
        OrganizationId organizationId,
        PersonId studentId,
        Enrollment enrollment,
        CancellationToken ct)
    {
        var values = new Dictionary<string, PrerequisiteEvaluation>(StringComparer.OrdinalIgnoreCase);

        Student? student = await db.Students.AsNoTracking()
            .SingleOrDefaultAsync(
                s => s.OrganizationId == organizationId && s.Id == studentId,
                ct);

        values["IDENTITY"] = new PrerequisiteEvaluation(
            student?.IdentityVerificationStatus is IdentityVerificationStatus.DocumentVerified
                or IdentityVerificationStatus.ExternallyVerified
                ? ChecklistItemStatus.Completed
                : ChecklistItemStatus.NotStarted,
            student?.IdentityVerifiedAtUtc is { } verifiedAt
                ? $"student-identity:{verifiedAt:O}"
                : null);

        List<StudentDocument> identityDocuments = await db.StudentDocuments.AsNoTracking()
            .Where(d =>
                d.OrganizationId == organizationId
                && d.StudentId == studentId
                && (d.EnrollmentId == null || d.EnrollmentId == enrollment.Id)
                && d.Category == StudentDocumentCategory.Identity)
            .ToListAsync(ct);

        DateOnly today = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);
        StudentDocument? approvedIdentityDocument = identityDocuments
            .Where(d => d.Status == StudentDocumentStatus.Approved
                        && (!d.ExpiresOn.HasValue || d.ExpiresOn.Value >= today))
            .OrderByDescending(d => d.CurrentVersion)
            .FirstOrDefault();

        ChecklistItemStatus identityDocumentStatus = approvedIdentityDocument is not null
            ? ChecklistItemStatus.Completed
            : identityDocuments.Any(d => d.Status is StudentDocumentStatus.PendingReview
                or StudentDocumentStatus.Uploaded
                or StudentDocumentStatus.Processing)
                ? ChecklistItemStatus.InProgress
                : identityDocuments.Any(d =>
                    (d.Status is StudentDocumentStatus.Rejected or StudentDocumentStatus.Expired)
                    || (d.ExpiresOn.HasValue && d.ExpiresOn.Value < today))
                    ? ChecklistItemStatus.Blocked
                    : ChecklistItemStatus.NotStarted;

        values["IDENTITY_DOCUMENT"] = new PrerequisiteEvaluation(
            identityDocumentStatus,
            approvedIdentityDocument is null
                ? null
                : $"student-document:{approvedIdentityDocument.Id.Value}");

        EnrollmentPrerequisiteSnapshot external = await prerequisiteSnapshotProvider.GetAsync(
            organizationId,
            studentId,
            enrollment.Id,
            enrollment.SourceLeadId,
            ct);

        Add(values, "CONTRACT", external.Contract);
        Add(values, "INITIAL_PAYMENT", external.InitialPayment);
        Add(values, "INITIAL_ASSESSMENT", external.InitialAssessment);
        Add(values, "LEARNING_PATH", external.LearningPath);
        Add(values, "STUDENT_ACCOUNT", external.StudentAccount);

        return values;
    }

    private static void Add(
        IDictionary<string, PrerequisiteEvaluation> values,
        string code,
        PrerequisiteEvaluation? evaluation)
    {
        if (evaluation is not null)
            values[code] = evaluation;
    }

    private static void ApplyDerivedStatuses(
        EnrollmentChecklist checklist,
        IReadOnlyDictionary<string, PrerequisiteEvaluation> derived,
        UserId actor,
        DateTimeOffset now)
    {
        foreach (EnrollmentChecklistItem item in checklist.Items)
        {
            if (!derived.TryGetValue(item.Code, out PrerequisiteEvaluation? evaluation))
                continue;

            // An authorized waiver is an explicit business decision and must not be
            // overwritten by a later synchronization. All other derived states follow
            // the source domain.
            if (item.Status == ChecklistItemStatus.Waived)
                continue;

            checklist.ChangeStatus(
                item.Id,
                evaluation.Status,
                evaluation.EvidenceReference,
                actor,
                now);
        }
    }

    private async Task<Result> Change(
        OrganizationId org,
        PersonId studentId,
        DraftEnrollmentId enrollmentId,
        Func<EnrollmentChecklist, Result> action,
        CancellationToken ct)
    {
        EnrollmentChecklist? checklist = await Query(true)
            .SingleOrDefaultAsync(
                x => x.OrganizationId == org
                     && x.StudentId == studentId
                     && x.EnrollmentId == enrollmentId,
                ct);
        if (checklist is null)
            return Result.Failure(EnrollmentChecklistApplicationErrors.ChecklistNotFound);

        Result result = action(checklist);
        if (result.IsSuccess)
            await db.SaveChangesAsync(ct);
        return result;
    }

    private IQueryable<EnrollmentChecklist> Query(bool tracked) =>
        tracked
            ? db.EnrollmentChecklists.Include(x => x.Items)
            : db.EnrollmentChecklists.AsNoTracking().Include(x => x.Items);

    private async Task<EnrollmentChecklist> GetOrCreate(
        OrganizationId org,
        PersonId studentId,
        DraftEnrollmentId enrollmentId,
        CancellationToken ct)
    {
        EnrollmentChecklist? checklist = await Query(true)
            .SingleOrDefaultAsync(
                x => x.OrganizationId == org
                     && x.StudentId == studentId
                     && x.EnrollmentId == enrollmentId,
                ct);
        if (checklist is not null)
            return checklist;

        checklist = EnrollmentChecklist.Create(org, studentId, enrollmentId).Value;
        db.EnrollmentChecklists.Add(checklist);
        return checklist;
    }

    private async Task<Enrollment?> Enrollment(
        OrganizationId org,
        PersonId studentId,
        DraftEnrollmentId? id,
        CancellationToken ct,
        bool tracked = false)
    {
        IQueryable<Enrollment> query = tracked ? db.Enrollments : db.Enrollments.AsNoTracking();
        query = query.Where(e => e.OrganizationId == org && e.StudentId == studentId);
        return id.HasValue
            ? await query.SingleOrDefaultAsync(e => e.Id == id.Value, ct)
            : await query.OrderByDescending(e => e.CreatedAtUtc).FirstOrDefaultAsync(ct);
    }

    private static EnrollmentChecklistResponse Empty(
        PersonId studentId,
        DraftEnrollmentId enrollmentId) =>
        new(studentId.Value, enrollmentId.Value, false, 0, 0, []);

    private static EnrollmentChecklistResponse Map(
        EnrollmentChecklist checklist,
        IReadOnlyDictionary<string, PrerequisiteEvaluation> derived)
    {
        ChecklistItemResponse[] items = checklist.Items
            .OrderBy(x => x.Category)
            .ThenBy(x => x.Code)
            .Select(x =>
            {
                bool isDerived = DerivedCodes.Contains(x.Code);
                ChecklistItemStatus status = x.Status == ChecklistItemStatus.Waived
                    ? ChecklistItemStatus.Waived
                    : derived.TryGetValue(x.Code, out PrerequisiteEvaluation? evaluation)
                        ? evaluation.Status
                        : x.Status;

                return new ChecklistItemResponse(
                    x.Id,
                    x.RuleId,
                    x.Code,
                    x.LabelKey,
                    $"students.checklistHelp.{x.Code}.description",
                    $"students.checklistHelp.{x.Code}.impact",
                    $"students.checklistHelp.{x.Code}.action",
                    x.Category,
                    x.IsBlocking,
                    isDerived,
                    NormalizeTargetRoute(x.Code, x.TargetRoute),
                    status,
                    x.ResponsibleUserId,
                    x.DueAtUtc,
                    x.Status == ChecklistItemStatus.Waived ? x.DecisionReason : null,
                    x.ReminderCount,
                    x.LastReminderAtUtc);
            })
            .ToArray();

        int total = items.Count(x => x.IsBlocking);
        int done = items.Count(x =>
            x.IsBlocking
            && x.Status is ChecklistItemStatus.Completed or ChecklistItemStatus.Waived);
        bool canActivate = total > 0 && done == total;

        return new EnrollmentChecklistResponse(
            checklist.StudentId.Value,
            checklist.EnrollmentId.Value,
            canActivate,
            done,
            total,
            items);
    }

    private static string NormalizeTargetRoute(string code, string configuredRoute) =>
        code.ToUpperInvariant() switch
        {
            "IDENTITY" => "profile",
            "IDENTITY_DOCUMENT" => "enrollment/documents",
            "CONTRACT" => "contracts",
            "INITIAL_PAYMENT" => "finance",
            "INITIAL_ASSESSMENT" => "pedagogy",
            "LEARNING_PATH" => "pedagogy",
            "STUDENT_ACCOUNT" => "profile",
            _ => configuredRoute,
        };

    private static IEnumerable<EnrollmentChecklistRule> Baseline(OrganizationId org)
    {
        yield return EnrollmentChecklistRule.Create(
            org, "*", "IDENTITY", "students.checklist.identity",
            ChecklistCategory.Identity, true, "profile", 3);
        yield return EnrollmentChecklistRule.Create(
            org, "*", "INITIAL_ASSESSMENT", "students.checklist.initialAssessment",
            ChecklistCategory.Pedagogy, true, "pedagogy", 7);
        yield return EnrollmentChecklistRule.Create(
            org, "*", "CONTRACT", "students.checklist.contract",
            ChecklistCategory.Contract, true, "contracts", 7);
        yield return EnrollmentChecklistRule.Create(
            org, "*", "INITIAL_PAYMENT", "students.checklist.initialPayment",
            ChecklistCategory.Finance, true, "finance", 7);
        yield return EnrollmentChecklistRule.Create(
            org, "*", "IDENTITY_DOCUMENT", "students.checklist.identityDocument",
            ChecklistCategory.Documents, true, "enrollment/documents", 5);
        yield return EnrollmentChecklistRule.Create(
            org, "*", "LEARNING_PATH", "students.checklist.learningPath",
            ChecklistCategory.Pedagogy, true, "pedagogy", 10);
        yield return EnrollmentChecklistRule.Create(
            org, "*", "STUDENT_ACCOUNT", "students.checklist.studentAccount",
            ChecklistCategory.UserAccount, false, "profile", 3);
    }
}
