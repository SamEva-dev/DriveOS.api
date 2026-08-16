using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Domain.Activities;

public sealed class CrmActivity : AggregateRoot<CrmActivityId>, IAuditableEntity
{
    private CrmActivity() { }

    private CrmActivity(
        CrmActivityId id,
        OrganizationId organizationId,
        LeadId? leadId,
        CrmActivityType type,
        CrmActivityDirection direction,
        string subject,
        string? details,
        DateTimeOffset occurredAtUtc,
        UserId? advisorUserId,
        CrmActivityMetadata metadata
    )
        : base(id)
    {
        OrganizationId = organizationId;
        LeadId = leadId;
        Type = type;
        Direction = direction;
        Subject = subject;
        Details = details;
        OccurredAtUtc = occurredAtUtc;
        AdvisorUserId = advisorUserId;
        Metadata = metadata;
    }

    public OrganizationId OrganizationId { get; private set; }
    public LeadId? LeadId { get; private set; }
    public CrmActivityType Type { get; private set; }
    public CrmActivityDirection Direction { get; private set; }
    public string Subject { get; private set; } = string.Empty;
    public string? Details { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }
    public UserId? AdvisorUserId { get; private set; }
    public CrmActivityMetadata Metadata { get; private set; } = null!;
    public DateTimeOffset? InvalidatedAtUtc { get; private set; }
    public UserId? InvalidatedByUserId { get; private set; }
    public string? InvalidationReason { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<CrmActivity> Create(
        CrmActivityId id,
        OrganizationId organizationId,
        LeadId? leadId,
        CrmActivityType type,
        CrmActivityDirection direction,
        string subject,
        string? details,
        DateTimeOffset occurredAtUtc,
        UserId? advisorUserId = null,
        CrmActivityMetadata? metadata = null
    )
    {
        if (id.IsEmpty)
            return Result.Failure<CrmActivity>(CrmActivityErrors.IdInvalid);
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
        if (leadId is { IsEmpty: true })
            return Result.Failure<CrmActivity>(CrmActivityErrors.LeadIdInvalid);
        if (metadata?.DurationMinutes is < 0 or > 1440)
            return Result.Failure<CrmActivity>(CrmActivityErrors.DurationInvalid);
        if (
            metadata?.Result?.Length > 100
            || metadata?.AttachmentName?.Length > 255
            || metadata?.AttachmentReference?.Length > 1000
        )
            return Result.Failure<CrmActivity>(CrmActivityErrors.MetadataInvalid);
        if (
            metadata?.Origin == CrmActivityOrigin.Imported
            && (
                string.IsNullOrWhiteSpace(metadata.ExternalId)
                || string.IsNullOrWhiteSpace(metadata.IdempotencyKey)
                || metadata.SyncStatus == CrmActivitySyncStatus.NotApplicable
                || metadata.SyncStatus == CrmActivitySyncStatus.Failed
                    && string.IsNullOrWhiteSpace(metadata.SyncErrorKey)
            )
        )
            return Result.Failure<CrmActivity>(CrmActivityErrors.MetadataInvalid);

        return Result.Success(
            new CrmActivity(
                id,
                organizationId,
                leadId,
                type,
                direction,
                normalizedSubject,
                normalizedDetails,
                occurredAtUtc.ToUniversalTime(),
                advisorUserId,
                metadata ?? CrmActivityMetadata.Manual()
            )
        );
    }

    public Result AttachToLead(LeadId leadId)
    {
        if (leadId.IsEmpty)
            return Result.Failure(CrmActivityErrors.LeadIdInvalid);
        if (LeadId.HasValue)
            return Result.Failure(CrmActivityErrors.AlreadyAttached);
        LeadId = leadId;
        return Result.Success();
    }

    public Result Invalidate(string reason, UserId userId, DateTimeOffset nowUtc)
    {
        if (InvalidatedAtUtc.HasValue)
            return Result.Failure(CrmActivityErrors.AlreadyInvalidated);
        string value = reason?.Trim() ?? string.Empty;
        if (value.Length is < 3 or > 500)
            return Result.Failure(CrmActivityErrors.InvalidationReasonInvalid);
        InvalidatedAtUtc = nowUtc.ToUniversalTime();
        InvalidatedByUserId = userId;
        InvalidationReason = value;
        return Result.Success();
    }

    public Result RetrySynchronization(DateTimeOffset nowUtc)
    {
        if (Metadata.SyncStatus != CrmActivitySyncStatus.Failed)
            return Result.Failure(CrmActivityErrors.SyncRetryNotAllowed);
        Metadata = Metadata with
        {
            SyncStatus = CrmActivitySyncStatus.Pending,
            SyncAttemptCount = Metadata.SyncAttemptCount + 1,
            LastSyncAttemptAtUtc = nowUtc,
            SyncErrorKey = null,
        };
        return Result.Success();
    }

    public Result AbandonSynchronization(DateTimeOffset nowUtc)
    {
        if (
            Metadata.SyncStatus
            is not (CrmActivitySyncStatus.Failed or CrmActivitySyncStatus.Pending)
        )
            return Result.Failure(CrmActivityErrors.SyncAbandonNotAllowed);
        Metadata = Metadata with
        {
            SyncStatus = CrmActivitySyncStatus.Abandoned,
            LastSyncAttemptAtUtc = nowUtc,
        };
        return Result.Success();
    }

    public Result SetAttachment(string fileName, string opaqueReference)
    {
        string name = Path.GetFileName(fileName?.Trim() ?? string.Empty);
        string reference = opaqueReference?.Trim() ?? string.Empty;
        if (name.Length is < 1 or > 255 || reference.Length is < 1 or > 1000)
            return Result.Failure(CrmActivityErrors.AttachmentInvalid);
        Metadata = Metadata with { AttachmentName = name, AttachmentReference = reference };
        return Result.Success();
    }

    public Result RemoveAttachment()
    {
        if (string.IsNullOrWhiteSpace(Metadata.AttachmentReference))
            return Result.Failure(CrmActivityErrors.AttachmentNotFound);
        Metadata = Metadata with { AttachmentName = null, AttachmentReference = null };
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
