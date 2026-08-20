using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Students.Domain.Checklists;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Application.Checklists;

public sealed record EnrollmentChecklistResponse(
    Guid StudentId,
    Guid EnrollmentId,
    bool CanActivate,
    int CompletedBlocking,
    int TotalBlocking,
    IReadOnlyList<ChecklistItemResponse> Items
);

public sealed record ChecklistItemResponse(
    Guid Id,
    Guid RuleId,
    string Code,
    string LabelKey,
    string DescriptionKey,
    string ImpactKey,
    string ActionLabelKey,
    ChecklistCategory Category,
    bool IsBlocking,
    bool IsDerived,
    string TargetRoute,
    ChecklistItemStatus Status,
    Guid? ResponsibleUserId,
    DateTimeOffset? DueAtUtc,
    string? DecisionReason,
    int ReminderCount,
    DateTimeOffset? LastReminderAtUtc
);

public sealed record GetEnrollmentChecklistQuery(
    OrganizationId OrganizationId,
    PersonId StudentId,
    DraftEnrollmentId? EnrollmentId
) : IQuery<EnrollmentChecklistResponse>;

public sealed record SynchronizeEnrollmentChecklistCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    DraftEnrollmentId EnrollmentId,
    UserId ActorUserId
) : ICommand<int>;

public sealed record ChangeChecklistItemStatusCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    DraftEnrollmentId EnrollmentId,
    Guid ItemId,
    ChecklistItemStatus Status,
    string? Reason,
    bool CanApproveException,
    UserId ActorUserId
) : ICommand;

public sealed record AssignChecklistItemCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    DraftEnrollmentId EnrollmentId,
    Guid ItemId,
    Guid ResponsibleUserId,
    UserId ActorUserId
) : ICommand;

public sealed record RemindChecklistItemCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    DraftEnrollmentId EnrollmentId,
    Guid ItemId,
    UserId ActorUserId
) : ICommand;

public sealed record ActivateEnrollmentCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    DraftEnrollmentId EnrollmentId,
    UserId ActorUserId
) : ICommand;

public sealed record ConfigureChecklistRuleCommand(
    OrganizationId OrganizationId,
    Guid? RuleId,
    string TrainingCode,
    string Code,
    string LabelKey,
    ChecklistCategory Category,
    bool IsBlocking,
    string TargetRoute,
    int DueInDays,
    bool IsActive
) : ICommand<Guid>;

public interface IEnrollmentChecklistService
{
    Task<EnrollmentChecklistResponse?> GetAsync(
        GetEnrollmentChecklistQuery query,
        CancellationToken ct = default
    );
    Task<Result<int>> SynchronizeAsync(
        SynchronizeEnrollmentChecklistCommand command,
        CancellationToken ct = default
    );
    Task<Result> ChangeStatusAsync(
        ChangeChecklistItemStatusCommand command,
        CancellationToken ct = default
    );
    Task<Result> AssignAsync(AssignChecklistItemCommand command, CancellationToken ct = default);
    Task<Result> RemindAsync(RemindChecklistItemCommand command, CancellationToken ct = default);
    Task<Result> ActivateAsync(ActivateEnrollmentCommand command, CancellationToken ct = default);
    Task<Result<Guid>> ConfigureRuleAsync(
        ConfigureChecklistRuleCommand command,
        CancellationToken ct = default
    );
}

public static class EnrollmentChecklistApplicationErrors
{
    public static readonly Error EnrollmentNotFound = Error.NotFound(
        "Students.Checklist.Enrollment.NotFound",
        "errors.students.checklist.enrollment.notFound"
    );
    public static readonly Error ChecklistNotFound = Error.NotFound(
        "Students.Checklist.NotFound",
        "errors.students.checklist.notFound"
    );
    public static readonly Error ExceptionApprovalForbidden = Error.Forbidden(
        "Students.Checklist.Exception.Forbidden",
        "errors.students.checklist.exception.forbidden"
    );
    public static readonly Error RuleNotFound = Error.NotFound(
        "Students.Checklist.Rule.NotFound",
        "errors.students.checklist.rule.notFound"
    );
    public static readonly Error DerivedStatusManualChangeForbidden = Error.Conflict(
        "Students.Checklist.DerivedStatus.ManualChangeForbidden",
        "errors.students.checklist.derivedStatus.manualChangeForbidden"
    );
}
