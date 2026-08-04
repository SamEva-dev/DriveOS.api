using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain.Subscriptions;

public static class OrganizationSubscriptionErrors
{
    public static readonly Error EmptyId = Error.Validation(
        "OrganizationSubscriptions.Id.Empty",
        "errors.organizationSubscription.id.empty");

    public static readonly Error EmptyOrganizationId = Error.Validation(
        "OrganizationSubscriptions.OrganizationId.Empty",
        "errors.organizationSubscription.organizationId.empty");

    public static readonly Error EmptyPlanCode = Error.Validation(
        "OrganizationSubscriptions.PlanCode.Empty",
        "errors.organizationSubscription.planCode.empty");

    public static Error PlanCodeTooLong(int maximumLength) => Error.Validation(
        "OrganizationSubscriptions.PlanCode.TooLong",
        "errors.organizationSubscription.planCode.tooLong",
        new Dictionary<string, object?> { ["maximumLength"] = maximumLength });

    public static readonly Error InvalidPeriod = Error.Validation(
        "OrganizationSubscriptions.Period.Invalid",
        "errors.organizationSubscription.invalidPeriod");

    public static readonly Error InvalidTrialPeriod = Error.Validation(
        "OrganizationSubscriptions.TrialPeriod.Invalid",
        "errors.organizationSubscription.invalidTrialPeriod");

    public static readonly Error InvalidCancellationDate = Error.Validation(
        "OrganizationSubscriptions.Cancellation.Date.Invalid",
        "errors.organizationSubscription.cancellation.invalidDate");

    public static readonly Error EmptyChangeReason = Error.Validation(
        "OrganizationSubscriptions.ChangeReason.Empty",
        "errors.organizationSubscription.changeReason.required");

    public static Error ChangeReasonTooLong(int maximumLength) => Error.Validation(
        "OrganizationSubscriptions.ChangeReason.TooLong",
        "errors.organizationSubscription.changeReason.tooLong",
        new Dictionary<string, object?> { ["maximumLength"] = maximumLength });

    public static readonly Error EmptyActorUserId = Error.Validation(
        "OrganizationSubscriptions.ActorUserId.Empty",
        "errors.organizationSubscription.actorUserId.empty");

    public static readonly Error InvalidEntitlementCode = Error.Validation(
        "OrganizationSubscriptions.Entitlement.Code.Invalid",
        "errors.organizationSubscription.entitlement.invalidCode");

    public static readonly Error DuplicateEntitlement = Error.Conflict(
        "OrganizationSubscriptions.Entitlement.Duplicate",
        "errors.organizationSubscription.entitlement.duplicate");

    public static readonly Error InvalidLimitCode = Error.Validation(
        "OrganizationSubscriptions.Limit.Code.Invalid",
        "errors.organizationSubscription.limit.invalidCode");

    public static readonly Error InvalidLimitValue = Error.Validation(
        "OrganizationSubscriptions.Limit.Value.Invalid",
        "errors.organizationSubscription.limit.invalidValue");

    public static readonly Error DuplicateLimit = Error.Conflict(
        "OrganizationSubscriptions.Limit.Duplicate",
        "errors.organizationSubscription.limit.duplicate");

    public static readonly Error InvalidStatusTransition = Error.Conflict(
        "OrganizationSubscriptions.Status.InvalidTransition",
        "errors.organizationSubscription.invalidStatusTransition");

    public static readonly Error CancelledSubscriptionCannotBeChanged = Error.Conflict(
        "OrganizationSubscriptions.Cancelled.CannotBeChanged",
        "errors.organizationSubscription.cancelled.cannotBeChanged");

    public static readonly Error NotFound = Error.NotFound(
        "OrganizationSubscriptions.NotFound",
        "errors.organizationSubscription.notFound");

    public static readonly Error AlreadyExists = Error.Conflict(
        "OrganizationSubscriptions.AlreadyExists",
        "errors.organizationSubscription.alreadyExists");

    public static readonly Error ExternalReferenceAlreadyUsed = Error.Conflict(
        "OrganizationSubscriptions.ExternalReference.AlreadyUsed",
        "errors.organizationSubscription.externalReferenceAlreadyUsed");
    public static readonly Error ConcurrentUpdate = Error.Conflict(
        "OrganizationSubscriptions.ConcurrentUpdate",
        "errors.organizationSubscription.concurrentUpdate");
}
