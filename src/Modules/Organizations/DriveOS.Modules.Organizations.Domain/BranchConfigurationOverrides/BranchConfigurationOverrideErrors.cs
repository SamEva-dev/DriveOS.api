using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.Organizations.Domain.BranchConfigurationOverrides;
public static class BranchConfigurationOverrideErrors
{
    public static readonly Error EmptyId = Error.Validation("BranchConfigurationOverrides.Id.Empty", "errors.branchConfigurationOverride.id.empty");
    public static readonly Error EmptyOrganizationId = Error.Validation("BranchConfigurationOverrides.OrganizationId.Empty", "errors.branchConfigurationOverride.organizationId.empty");
    public static readonly Error EmptyBranchId = Error.Validation("BranchConfigurationOverrides.BranchId.Empty", "errors.branchConfigurationOverride.branchId.empty");
    public static readonly Error EmptyBaseConfigurationId = Error.Validation("BranchConfigurationOverrides.BaseConfigurationId.Empty", "errors.branchConfigurationOverride.baseConfigurationId.empty");
    public static readonly Error InvalidVersion = Error.Validation("BranchConfigurationOverrides.Version.Invalid", "errors.branchConfigurationOverride.version.invalid");
    public static readonly Error InvalidCountryCode = Error.Validation("BranchConfigurationOverrides.CountryCode.Invalid", "errors.branchConfigurationOverride.countryCode.invalid");
    public static readonly Error EmptyPayload = Error.Validation("BranchConfigurationOverrides.Payload.Empty", "errors.branchConfigurationOverride.payload.empty");
    public static readonly Error PayloadTooLarge = Error.Validation("BranchConfigurationOverrides.Payload.TooLarge", "errors.branchConfigurationOverride.payload.tooLarge");
    public static readonly Error InvalidJson = Error.Validation("BranchConfigurationOverrides.Payload.InvalidJson", "errors.branchConfigurationOverride.payload.invalidJson");
    public static readonly Error PayloadRootMustBeObject = Error.Validation("BranchConfigurationOverrides.Payload.MustBeObject", "errors.branchConfigurationOverride.payload.mustBeObject");
    public static readonly Error DraftRequired = Error.Conflict("BranchConfigurationOverrides.Draft.Required", "errors.branchConfigurationOverride.draft.required");
    public static readonly Error PublishedRequired = Error.Conflict("BranchConfigurationOverrides.Published.Required", "errors.branchConfigurationOverride.published.required");
    public static readonly Error AlreadyPublished = Error.Conflict("BranchConfigurationOverrides.AlreadyPublished", "errors.branchConfigurationOverride.alreadyPublished");
    public static readonly Error InvalidEffectivePeriod = Error.Validation("BranchConfigurationOverrides.EffectivePeriod.Invalid", "errors.branchConfigurationOverride.effectivePeriod.invalid");
    public static readonly Error EmptyPublisher = Error.Validation("BranchConfigurationOverrides.Publisher.Empty", "errors.branchConfigurationOverride.publisher.empty");
    public static readonly Error VersionAlreadyExists = Error.Conflict("BranchConfigurationOverrides.Version.AlreadyExists", "errors.branchConfigurationOverride.version.alreadyExists");
    public static readonly Error NotFound = Error.NotFound("BranchConfigurationOverrides.NotFound", "errors.branchConfigurationOverride.notFound");
    public static readonly Error ConcurrentUpdate = Error.Conflict("BranchConfigurationOverrides.ConcurrentUpdate", "errors.branchConfigurationOverride.concurrentUpdate");
    public static readonly Error CurrentUserRequired = Error.Unauthorized("BranchConfigurationOverrides.CurrentUser.Required", "errors.branchConfigurationOverride.currentUser.required");
    public static readonly Error BaseConfigurationNotFound = Error.NotFound("BranchConfigurationOverrides.BaseConfiguration.NotFound", "errors.branchConfigurationOverride.baseConfiguration.notFound");
    public static readonly Error BaseConfigurationMustBePublished = Error.Conflict("BranchConfigurationOverrides.BaseConfiguration.PublishedRequired", "errors.branchConfigurationOverride.baseConfiguration.publishedRequired");
    public static readonly Error CountryCodeMismatch = Error.Validation("BranchConfigurationOverrides.CountryCode.Mismatch", "errors.branchConfigurationOverride.countryCode.mismatch");
    public static readonly Error OutsideBaseConfigurationPeriod = Error.Validation("BranchConfigurationOverrides.EffectivePeriod.OutsideBase", "errors.branchConfigurationOverride.effectivePeriod.outsideBase");

    public static Error ContainsLockedOrUnauthorizedPaths(IEnumerable<string> paths) => new(
        "BranchConfigurationOverrides.OverridePaths.NotAllowed",
        $"errors.branchConfigurationOverride.overridePaths.notAllowed:{string.Join(",", paths)}",
        ErrorType.Validation);
}
