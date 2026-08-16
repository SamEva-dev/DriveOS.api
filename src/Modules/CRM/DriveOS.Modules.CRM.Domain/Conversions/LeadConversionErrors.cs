using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Domain.Conversions;

public static class LeadConversionErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Crm.Conversions.NotFound",
        "errors.crm.conversions.notFound"
    );
    public static readonly Error InvalidCompletionTarget = Error.Validation(
        "Crm.Conversions.CompletionTarget.Invalid",
        "errors.crm.conversions.completionTarget.invalid"
    );
    public static readonly Error AlreadyCompleted = Error.Conflict(
        "Crm.Conversions.AlreadyCompleted",
        "errors.crm.conversions.alreadyCompleted"
    );
}
