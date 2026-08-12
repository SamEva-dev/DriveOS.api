using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Domain.Activities;

public static class CrmActivityErrors
{
    public static readonly Error InvalidIdentifier = Error.Validation("Crm.Activities.Identifier.Invalid", "errors.crm.activities.identifier.invalid");
    public static readonly Error SubjectRequired = Error.Validation("Crm.Activities.Subject.Required", "errors.crm.activities.subject.required");
    public static readonly Error SubjectTooLong = Error.Validation("Crm.Activities.Subject.TooLong", "errors.crm.activities.subject.tooLong");
    public static readonly Error DetailsTooLong = Error.Validation("Crm.Activities.Details.TooLong", "errors.crm.activities.details.tooLong");
    public static readonly Error OccurredAtRequired = Error.Validation("Crm.Activities.OccurredAt.Required", "errors.crm.activities.occurredAt.required");
    public static readonly Error DirectionNotAllowed = Error.Validation("Crm.Activities.Direction.NotAllowed", "errors.crm.activities.direction.notAllowed");
}
