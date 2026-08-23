using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain.RegulatoryIntegrations;

public static class RegulatoryIntegrationConnectionErrors
{
    public static readonly Error NotFound = Error.NotFound("RegulatoryIntegrations.NotFound", "errors.regulatoryIntegrations.notFound");
    public static readonly Error AlreadyExists = Error.Conflict("RegulatoryIntegrations.AlreadyExists", "errors.regulatoryIntegrations.alreadyExists");
    public static readonly Error InvalidCountryCode = Error.Validation("RegulatoryIntegrations.CountryCode.Invalid", "errors.regulatoryIntegrations.countryCode.invalid");
    public static readonly Error InvalidProviderCode = Error.Validation("RegulatoryIntegrations.ProviderCode.Invalid", "errors.regulatoryIntegrations.providerCode.invalid");
    public static readonly Error InvalidExternalAccountReference = Error.Validation("RegulatoryIntegrations.ExternalAccountReference.Invalid", "errors.regulatoryIntegrations.externalAccountReference.invalid");
    public static readonly Error InvalidSecretReference = Error.Validation("RegulatoryIntegrations.SecretReference.Invalid", "errors.regulatoryIntegrations.secretReference.invalid");
    public static readonly Error BranchNotOwned = Error.Validation("RegulatoryIntegrations.Branch.NotOwned", "errors.regulatoryIntegrations.branch.notOwned");
    public static readonly Error ConcurrentUpdate = Error.Conflict("RegulatoryIntegrations.ConcurrentUpdate", "errors.regulatoryIntegrations.concurrentUpdate");
    public static readonly Error Ended = Error.Conflict("RegulatoryIntegrations.Ended", "errors.regulatoryIntegrations.ended");
}
