using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Domain.Activities;

public sealed class CrmActivity : AggregateRoot<CrmActivityId>, IAuditableEntity
{
    private CrmActivity() { }

    private CrmActivity(CrmActivityId id, OrganizationId organizationId, LeadId leadId,
        CrmActivityType type, CrmActivityDirection direction, string subject,
        string? details, DateTimeOffset occurredAtUtc)
        : base(id)
    {
        OrganizationId = organizationId;
        LeadId = leadId;
        Type = type;
        Direction = direction;
        Subject = subject;
        Details = details;
        OccurredAtUtc = occurredAtUtc;
    }

    public OrganizationId OrganizationId { get; private set; }
    public LeadId LeadId { get; private set; }
    public CrmActivityType Type { get; private set; }
    public CrmActivityDirection Direction { get; private set; }
    public string Subject { get; private set; } = string.Empty;
    public string? Details { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<CrmActivity> Create(CrmActivityId id, OrganizationId organizationId,
        LeadId leadId, CrmActivityType type, CrmActivityDirection direction,
        string subject, string? details, DateTimeOffset occurredAtUtc)
    {
        if (id.IsEmpty)
            return Result.Failure<CrmActivity>(CrmActivityErrors.InvalidIdentifier);

        string normalizedSubject = subject?.Trim() ?? string.Empty;
        string? normalizedDetails = string.IsNullOrWhiteSpace(details) ? null : details.Trim();

        if (normalizedSubject.Length == 0)
            return Result.Failure<CrmActivity>(CrmActivityErrors.SubjectRequired);
        if (normalizedSubject.Length > 200)
            return Result.Failure<CrmActivity>(CrmActivityErrors.SubjectTooLong);
        if (normalizedDetails?.Length > 4000)
            return Result.Failure<CrmActivity>(CrmActivityErrors.DetailsTooLong);
        if (occurredAtUtc == default)
            return Result.Failure<CrmActivity>(CrmActivityErrors.OccurredAtRequired);
        if (type == CrmActivityType.Note && direction != CrmActivityDirection.None)
            return Result.Failure<CrmActivity>(CrmActivityErrors.DirectionNotAllowed);

        var activity = new CrmActivity(id, organizationId, leadId, type,
            direction, normalizedSubject, normalizedDetails, occurredAtUtc.ToUniversalTime());
        activity.RaiseDomainEvent(new Events.LeadActivityCreatedDomainEvent(
            activity.Id, activity.OrganizationId, activity.LeadId, activity.Type,
            activity.OccurredAtUtc));
        return Result.Success(activity);
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
