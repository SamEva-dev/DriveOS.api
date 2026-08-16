using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using DriveOS.Modules.Students.Domain.Events;

namespace DriveOS.Modules.Students.Domain.Checklists;

public sealed class EnrollmentChecklist : AggregateRoot<EnrollmentChecklistId>
{
    private readonly List<EnrollmentChecklistItem> items = [];

    private EnrollmentChecklist() { }

    private EnrollmentChecklist(
        EnrollmentChecklistId id,
        OrganizationId org,
        PersonId studentId,
        DraftEnrollmentId enrollmentId
    )
        : base(id)
    {
        OrganizationId = org;
        StudentId = studentId;
        EnrollmentId = enrollmentId;
        RaiseDomainEvent(new StudentAggregateChangedDomainEvent<EnrollmentChecklistId>(Id, StudentId, OrganizationId, "Created"));
    }

    public OrganizationId OrganizationId { get; private set; }
    public PersonId StudentId { get; private set; }
    public DraftEnrollmentId EnrollmentId { get; private set; }
    public IReadOnlyCollection<EnrollmentChecklistItem> Items => items;

    public static Result<EnrollmentChecklist> Create(
        OrganizationId org,
        PersonId studentId,
        DraftEnrollmentId enrollmentId
    )
    {
        if (org.IsEmpty || studentId.IsEmpty || enrollmentId.IsEmpty)
            return Result.Failure<EnrollmentChecklist>(EnrollmentChecklistErrors.InvalidOwner);
        return Result.Success(
            new EnrollmentChecklist(EnrollmentChecklistId.New(), org, studentId, enrollmentId)
        );
    }

    public Result<Guid> UpsertRule(
        Guid ruleId,
        string code,
        string labelKey,
        ChecklistCategory category,
        bool blocking,
        string targetRoute,
        Guid? responsible,
        DateTimeOffset? due,
        UserId actor,
        DateTimeOffset now
    )
    {
        if (
            ruleId == Guid.Empty
            || string.IsNullOrWhiteSpace(code)
            || string.IsNullOrWhiteSpace(labelKey)
            || string.IsNullOrWhiteSpace(targetRoute)
        )
            return Result.Failure<Guid>(EnrollmentChecklistErrors.InvalidItem);
        var item = items.SingleOrDefault(x => x.RuleId == ruleId);
        if (item is null)
        {
            item = EnrollmentChecklistItem.Create(
                Id,
                ruleId,
                code,
                labelKey,
                category,
                blocking,
                targetRoute,
                responsible,
                due,
                actor,
                now
            );
            items.Add(item);
        }
        else
            item.RefreshRule(code, labelKey, category, blocking, targetRoute, due, actor, now);
        return Result.Success(item.Id);
    }

    public Result ChangeStatus(
        Guid itemId,
        ChecklistItemStatus status,
        string? reason,
        UserId actor,
        DateTimeOffset now
    )
    {
        var item = items.SingleOrDefault(x => x.Id == itemId);
        if (item is null)
            return Result.Failure(EnrollmentChecklistErrors.ItemNotFound);
        if (
            (
                status
                is ChecklistItemStatus.Waived
                    or ChecklistItemStatus.Rejected
                    or ChecklistItemStatus.Blocked
            ) && string.IsNullOrWhiteSpace(reason)
        )
            return Result.Failure(EnrollmentChecklistErrors.ReasonRequired);
        item.ChangeStatus(status, reason, actor, now);
        return Result.Success();
    }

    public Result Assign(Guid itemId, Guid responsible, UserId actor, DateTimeOffset now)
    {
        var item = items.SingleOrDefault(x => x.Id == itemId);
        if (item is null)
            return Result.Failure(EnrollmentChecklistErrors.ItemNotFound);
        item.Assign(responsible, actor, now);
        return Result.Success();
    }

    public Result Remind(Guid itemId, UserId actor, DateTimeOffset now)
    {
        var item = items.SingleOrDefault(x => x.Id == itemId);
        if (item is null)
            return Result.Failure(EnrollmentChecklistErrors.ItemNotFound);
        item.Remind(actor, now);
        return Result.Success();
    }

    public bool CanActivate() =>
        items.Count > 0
        && items
            .Where(x => x.IsBlocking)
            .All(x => x.Status is ChecklistItemStatus.Completed or ChecklistItemStatus.Waived);
}

public sealed class EnrollmentChecklistItem
{
    private EnrollmentChecklistItem() { }

    private EnrollmentChecklistItem(
        Guid checklistId,
        Guid ruleId,
        string code,
        string labelKey,
        ChecklistCategory category,
        bool blocking,
        string route,
        Guid? responsible,
        DateTimeOffset? due,
        UserId actor,
        DateTimeOffset now
    )
    {
        Id = Guid.NewGuid();
        EnrollmentChecklistId = new EnrollmentChecklistId(checklistId);
        RuleId = ruleId;
        Code = code;
        LabelKey = labelKey;
        Category = category;
        IsBlocking = blocking;
        TargetRoute = route;
        ResponsibleUserId = responsible;
        DueAtUtc = due;
        Status = ChecklistItemStatus.NotStarted;
        ModifiedByUserId = actor;
        ModifiedAtUtc = now;
    }

    public Guid Id { get; private set; }
    public EnrollmentChecklistId EnrollmentChecklistId { get; private set; }
    public Guid RuleId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string LabelKey { get; private set; } = string.Empty;
    public ChecklistCategory Category { get; private set; }
    public bool IsBlocking { get; private set; }
    public string TargetRoute { get; private set; } = string.Empty;
    public ChecklistItemStatus Status { get; private set; }
    public Guid? ResponsibleUserId { get; private set; }
    public DateTimeOffset? DueAtUtc { get; private set; }
    public string? DecisionReason { get; private set; }
    public int ReminderCount { get; private set; }
    public DateTimeOffset? LastReminderAtUtc { get; private set; }
    public UserId ModifiedByUserId { get; private set; }
    public DateTimeOffset ModifiedAtUtc { get; private set; }

    internal static EnrollmentChecklistItem Create(
        Guid c,
        Guid r,
        string code,
        string label,
        ChecklistCategory category,
        bool blocking,
        string route,
        Guid? responsible,
        DateTimeOffset? due,
        UserId actor,
        DateTimeOffset now
    ) =>
        new(
            c,
            r,
            code.Trim(),
            label.Trim(),
            category,
            blocking,
            route.Trim(),
            responsible,
            due,
            actor,
            now
        );

    internal void RefreshRule(
        string code,
        string label,
        ChecklistCategory category,
        bool blocking,
        string route,
        DateTimeOffset? due,
        UserId actor,
        DateTimeOffset now
    )
    {
        Code = code.Trim();
        LabelKey = label.Trim();
        Category = category;
        IsBlocking = blocking;
        TargetRoute = route.Trim();
        DueAtUtc = due;
        Touch(actor, now);
    }

    internal void ChangeStatus(
        ChecklistItemStatus status,
        string? reason,
        UserId actor,
        DateTimeOffset now
    )
    {
        Status = status;
        DecisionReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        Touch(actor, now);
    }

    internal void Assign(Guid responsible, UserId actor, DateTimeOffset now)
    {
        ResponsibleUserId = responsible;
        Touch(actor, now);
    }

    internal void Remind(UserId actor, DateTimeOffset now)
    {
        ReminderCount++;
        LastReminderAtUtc = now;
        Touch(actor, now);
    }

    private void Touch(UserId actor, DateTimeOffset now)
    {
        ModifiedByUserId = actor;
        ModifiedAtUtc = now;
    }
}

public sealed class EnrollmentChecklistRule
{
    private EnrollmentChecklistRule() { }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public OrganizationId OrganizationId { get; private set; }
    public string TrainingCode { get; private set; } = "*";
    public string Code { get; private set; } = string.Empty;
    public string LabelKey { get; private set; } = string.Empty;
    public ChecklistCategory Category { get; private set; }
    public bool IsBlocking { get; private set; }
    public string TargetRoute { get; private set; } = string.Empty;
    public int DueInDays { get; private set; }
    public bool IsActive { get; private set; } = true;

    public static EnrollmentChecklistRule Create(
        OrganizationId org,
        string training,
        string code,
        string label,
        ChecklistCategory category,
        bool blocking,
        string route,
        int dueDays
    ) =>
        new()
        {
            OrganizationId = org,
            TrainingCode = string.IsNullOrWhiteSpace(training) ? "*" : training.Trim(),
            Code = code.Trim(),
            LabelKey = label.Trim(),
            Category = category,
            IsBlocking = blocking,
            TargetRoute = route.Trim(),
            DueInDays = Math.Max(0, dueDays),
        };

    public void Update(
        string label,
        ChecklistCategory category,
        bool blocking,
        string route,
        int dueDays,
        bool active
    )
    {
        LabelKey = label.Trim();
        Category = category;
        IsBlocking = blocking;
        TargetRoute = route.Trim();
        DueInDays = Math.Max(0, dueDays);
        IsActive = active;
    }
}
