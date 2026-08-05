using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain.OrganizationClosures;

public static class OrganizationClosureErrors
{
    public static readonly Error EmptyId = Error.Validation(
        "OrganizationClosures.Id.Empty",
        "errors.organizationClosures.id.empty");

    public static readonly Error EmptyOrganizationId = Error.Validation(
        "OrganizationClosures.OrganizationId.Empty",
        "errors.organizationClosures.organizationId.empty");

    public static readonly Error InvalidReason = Error.Validation(
        "OrganizationClosures.Reason.Invalid",
        "errors.organizationClosures.reason.invalid");

    public static readonly Error DetailsRequired = Error.Validation(
        "OrganizationClosures.Details.Required",
        "errors.organizationClosures.details.required");

    public static readonly Error DetailsTooLong = Error.Validation(
        "OrganizationClosures.Details.TooLong",
        "errors.organizationClosures.details.tooLong");

    public static readonly Error InvalidEffectiveDate = Error.Validation(
        "OrganizationClosures.EffectiveDate.Invalid",
        "errors.organizationClosures.effectiveDate.invalid");

    public static readonly Error InvalidRetentionDate = Error.Validation(
        "OrganizationClosures.RetentionDate.Invalid",
        "errors.organizationClosures.retentionDate.invalid");

    public static readonly Error InvalidStatusTransition = Error.Conflict(
        "OrganizationClosures.Status.InvalidTransition",
        "errors.organizationClosures.status.invalidTransition");

    public static readonly Error ActiveClosureAlreadyExists = Error.Conflict(
        "OrganizationClosures.Active.AlreadyExists",
        "errors.organizationClosures.active.alreadyExists");

        public static readonly Error NotFound = Error.NotFound(
        "OrganizationClosures.NotFound", "errors.organizationClosures.notFound");
    public static readonly Error CurrentUserRequired = Error.Unauthorized(
        "OrganizationClosures.CurrentUser.Required", "errors.authentication.required");
    public static readonly Error InvalidAction = Error.Validation(
        "OrganizationClosures.Action.Invalid", "errors.organizationClosures.action.invalid");
    public static readonly Error ReadinessBlocked = Error.Conflict(
        "OrganizationClosures.Readiness.Blocked", "errors.organizationClosures.readiness.blocked");
    public static readonly Error OrchestrationFailed = Error.Conflict(
        "OrganizationClosures.Orchestration.Failed", "errors.organizationClosures.orchestration.failed");

}
