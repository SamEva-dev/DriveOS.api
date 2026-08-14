using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Domain.Activities;

public static class CrmActivityErrors
{
    public static readonly Error IdInvalid = Error.Validation("Crm.Activities.Id.Invalid", "errors.crm.activities.id.invalid");
    public static readonly Error OccurredAtInFuture = Error.Validation("Crm.Activities.OccurredAt.InFuture", "errors.crm.activities.occurredAt.inFuture");
    public static readonly Error NextActionRequiresLead = Error.Validation("Crm.Activities.NextAction.RequiresLead", "errors.crm.activities.nextAction.requiresLead");
    public static readonly Error LeadIdInvalid = Error.Validation("Crm.Activities.LeadId.Invalid", "errors.crm.activities.leadId.invalid");
    public static readonly Error AlreadyAttached = Error.Conflict("Crm.Activities.AlreadyAttached", "errors.crm.activities.alreadyAttached");
    public static readonly Error DurationInvalid = Error.Validation("Crm.Activities.Duration.Invalid", "errors.crm.activities.duration.invalid");
    public static readonly Error MetadataInvalid = Error.Validation("Crm.Activities.Metadata.Invalid", "errors.crm.activities.metadata.invalid");
    public static readonly Error AlreadyInvalidated = Error.Conflict("Crm.Activities.AlreadyInvalidated", "errors.crm.activities.alreadyInvalidated");
    public static readonly Error InvalidationReasonInvalid = Error.Validation("Crm.Activities.InvalidationReason.Invalid", "errors.crm.activities.invalidationReason.invalid");
    public static readonly Error SyncRetryNotAllowed = Error.Conflict("Crm.Activities.SyncRetry.NotAllowed", "errors.crm.activities.syncRetry.notAllowed");
    public static readonly Error SyncAbandonNotAllowed = Error.Conflict("Crm.Activities.SyncAbandon.NotAllowed", "errors.crm.activities.syncAbandon.notAllowed");
    public static readonly Error SubjectRequired = Error.Validation("Crm.Activities.Subject.Required", "errors.crm.activities.subject.required");
    public static readonly Error SubjectTooLong = Error.Validation("Crm.Activities.Subject.TooLong", "errors.crm.activities.subject.tooLong");
    public static readonly Error DetailsTooLong = Error.Validation("Crm.Activities.Details.TooLong", "errors.crm.activities.details.tooLong");
    public static readonly Error OccurredAtRequired = Error.Validation("Crm.Activities.OccurredAt.Required", "errors.crm.activities.occurredAt.required");
    public static readonly Error DirectionNotAllowed = Error.Validation("Crm.Activities.Direction.NotAllowed", "errors.crm.activities.direction.notAllowed");
    public static readonly Error AttachmentInvalid = Error.Validation("Crm.Activities.Attachment.Invalid", "errors.crm.activities.attachment.invalid");
    public static readonly Error AttachmentNotFound = Error.NotFound("Crm.Activities.Attachment.NotFound", "errors.crm.activities.attachment.notFound");
    public static readonly Error AttachmentUnavailable = Error.Failure("Crm.Activities.Attachment.Unavailable", "errors.crm.activities.attachment.unavailable");
}
