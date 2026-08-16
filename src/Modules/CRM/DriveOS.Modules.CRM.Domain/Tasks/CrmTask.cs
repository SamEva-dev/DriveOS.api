using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Domain.Tasks;

public sealed class CrmTask : AggregateRoot<CrmTaskId>, IAuditableEntity
{
    private CrmTask() { }

    private CrmTask(
        CrmTaskId id,
        OrganizationId organizationId,
        LeadId leadId,
        CrmTaskType type,
        string title,
        string? notes,
        DateTimeOffset dueAtUtc,
        UserId? assignedToUserId
    )
        : base(id)
    {
        OrganizationId = organizationId;
        LeadId = leadId;
        Type = type;
        Title = title;
        Notes = notes;
        DueAtUtc = dueAtUtc;
        AssignedToUserId = assignedToUserId;
        Status = CrmTaskStatus.Pending;
    }

    public OrganizationId OrganizationId { get; private set; }
    public LeadId LeadId { get; private set; }
    public CrmTaskType Type { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Notes { get; private set; }
    public DateTimeOffset DueAtUtc { get; private set; }
    public UserId? AssignedToUserId { get; private set; }
    public CrmTaskStatus Status { get; private set; }
    public DateTimeOffset? ClosedAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<CrmTask> Create(
        CrmTaskId id,
        OrganizationId organizationId,
        LeadId leadId,
        CrmTaskType type,
        string title,
        string? notes,
        DateTimeOffset dueAtUtc,
        UserId? assignedToUserId
    )
    {
        if (id.IsEmpty)
            return Result.Failure<CrmTask>(CrmTaskErrors.IdInvalid);
        string normalizedTitle = title?.Trim() ?? string.Empty;
        string? normalizedNotes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        if (normalizedTitle.Length == 0)
            return Result.Failure<CrmTask>(CrmTaskErrors.TitleRequired);
        if (normalizedTitle.Length > 200)
            return Result.Failure<CrmTask>(CrmTaskErrors.TitleTooLong);
        if (normalizedNotes?.Length > 2000)
            return Result.Failure<CrmTask>(CrmTaskErrors.NotesTooLong);
        if (dueAtUtc == default)
            return Result.Failure<CrmTask>(CrmTaskErrors.DueDateRequired);
        return Result.Success(
            new CrmTask(
                id,
                organizationId,
                leadId,
                type,
                normalizedTitle,
                normalizedNotes,
                dueAtUtc.ToUniversalTime(),
                assignedToUserId
            )
        );
    }

    public Result Complete(DateTimeOffset now)
    {
        if (Status != CrmTaskStatus.Pending)
            return Result.Failure(CrmTaskErrors.AlreadyClosed);
        Status = CrmTaskStatus.Completed;
        ClosedAtUtc = now;
        return Result.Success();
    }

    public Result Cancel(DateTimeOffset now)
    {
        if (Status != CrmTaskStatus.Pending)
            return Result.Failure(CrmTaskErrors.AlreadyClosed);
        Status = CrmTaskStatus.Cancelled;
        ClosedAtUtc = now;
        return Result.Success();
    }

    public void SetCreatedAudit(DateTimeOffset createdAtUtc, UserId? createdByUserId)
    {
        if (CreatedAtUtc == default)
        {
            CreatedAtUtc = createdAtUtc;
            CreatedByUserId = createdByUserId;
        }
    }

    public void SetModifiedAudit(DateTimeOffset modifiedAtUtc, UserId? modifiedByUserId)
    {
        LastModifiedAtUtc = modifiedAtUtc;
        LastModifiedByUserId = modifiedByUserId;
    }
}
