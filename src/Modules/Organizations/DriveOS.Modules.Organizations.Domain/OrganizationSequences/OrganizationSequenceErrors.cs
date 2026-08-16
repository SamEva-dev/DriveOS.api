using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain.OrganizationSequences;

public static class OrganizationSequenceErrors
{
    public static readonly Error EmptyId = Error.Validation(
        "OrganizationSequences.Id.Empty",
        "errors.organizationSequence.id.empty"
    );

    public static readonly Error EmptyOrganizationId = Error.Validation(
        "OrganizationSequences.OrganizationId.Empty",
        "errors.organizationSequence.organizationId.empty"
    );

    public static readonly Error EmptyBranchId = Error.Validation(
        "OrganizationSequences.BranchId.Empty",
        "errors.organizationSequence.branchId.empty"
    );

    public static readonly Error BranchRequired = Error.Validation(
        "OrganizationSequences.Branch.Required",
        "errors.organizationSequence.branch.required"
    );

    public static readonly Error BranchNotAllowed = Error.Validation(
        "OrganizationSequences.Branch.NotAllowed",
        "errors.organizationSequence.branch.notAllowed"
    );

    public static readonly Error BranchNotFound = Error.NotFound(
        "OrganizationSequences.Branch.NotFound",
        "errors.organizationSequence.branch.notFound"
    );

    public static readonly Error OrganizationUnavailable = Error.Conflict(
        "OrganizationSequences.Organization.Unavailable",
        "errors.organizationSequence.organization.unavailable"
    );

    public static readonly Error InvalidScope = Error.Validation(
        "OrganizationSequences.Scope.Invalid",
        "errors.organizationSequence.scope.invalid"
    );

    public static readonly Error EmptyCode = Error.Validation(
        "OrganizationSequences.Code.Empty",
        "errors.organizationSequence.code.empty"
    );

    public static Error CodeTooLong(int maximumLength) =>
        Error.Validation(
            "OrganizationSequences.Code.TooLong",
            "errors.organizationSequence.code.tooLong",
            new Dictionary<string, object?> { ["maximumLength"] = maximumLength }
        );

    public static readonly Error InvalidCode = Error.Validation(
        "OrganizationSequences.Code.Invalid",
        "errors.organizationSequence.code.invalid"
    );

    public static readonly Error EmptyPattern = Error.Validation(
        "OrganizationSequences.Pattern.Empty",
        "errors.organizationSequence.pattern.empty"
    );

    public static Error PatternTooLong(int maximumLength) =>
        Error.Validation(
            "OrganizationSequences.Pattern.TooLong",
            "errors.organizationSequence.pattern.tooLong",
            new Dictionary<string, object?> { ["maximumLength"] = maximumLength }
        );

    public static readonly Error NumberTokenRequired = Error.Validation(
        "OrganizationSequences.Pattern.NumberTokenRequired",
        "errors.organizationSequence.pattern.numberTokenRequired"
    );

    public static readonly Error UnsupportedPatternToken = Error.Validation(
        "OrganizationSequences.Pattern.UnsupportedToken",
        "errors.organizationSequence.pattern.unsupportedToken"
    );

    public static readonly Error InvalidPattern = Error.Validation(
        "OrganizationSequences.Pattern.Invalid",
        "errors.organizationSequence.pattern.invalid"
    );

    public static readonly Error InvalidPadding = Error.Validation(
        "OrganizationSequences.Padding.Invalid",
        "errors.organizationSequence.padding.invalid"
    );

    public static readonly Error InvalidInitialValue = Error.Validation(
        "OrganizationSequences.InitialValue.Invalid",
        "errors.organizationSequence.initialValue.invalid"
    );

    public static readonly Error InvalidResetPolicy = Error.Validation(
        "OrganizationSequences.ResetPolicy.Invalid",
        "errors.organizationSequence.resetPolicy.invalid"
    );

    public static readonly Error ActiveRequired = Error.Conflict(
        "OrganizationSequences.Active.Required",
        "errors.organizationSequence.active.required"
    );

    public static readonly Error AlreadySuspended = Error.Conflict(
        "OrganizationSequences.AlreadySuspended",
        "errors.organizationSequence.alreadySuspended"
    );

    public static readonly Error ArchivedSequence = Error.Conflict(
        "OrganizationSequences.Archived",
        "errors.organizationSequence.archived"
    );

    public static readonly Error Archived = ArchivedSequence;

    public static readonly Error AlreadyExists = Error.Conflict(
        "OrganizationSequences.AlreadyExists",
        "errors.organizationSequence.alreadyExists"
    );

    public static readonly Error NotFound = Error.NotFound(
        "OrganizationSequences.NotFound",
        "errors.organizationSequence.notFound"
    );

    public static readonly Error CurrentUserRequired = Error.Unauthorized(
        "OrganizationSequences.CurrentUser.Required",
        "errors.organizationSequence.currentUser.required"
    );

    public static readonly Error ConcurrentUpdate = Error.Conflict(
        "OrganizationSequences.ConcurrentUpdate",
        "errors.organizationSequence.concurrentUpdate"
    );

    public static Error ConcurrencyRetryExhausted(int attempts) =>
        Error.Conflict(
            "OrganizationSequences.ConcurrencyRetryExhausted",
            "errors.organizationSequence.concurrencyRetryExhausted",
            new Dictionary<string, object?> { ["attempts"] = attempts }
        );
}
