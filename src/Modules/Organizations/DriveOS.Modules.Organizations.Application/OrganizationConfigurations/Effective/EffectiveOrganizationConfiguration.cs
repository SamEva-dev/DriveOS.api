using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationConfigurations.Effective;

public enum OrganizationConfigurationSource
{
    Organization = 1,
    BranchOverride = 2
}

public sealed record EffectiveOrganizationConfiguration(
    Guid ConfigurationId,
    OrganizationId OrganizationId,
    BranchId? BranchId,
    int VersionNumber,
    string CountryCode,
    string PayloadJson,
    DateTimeOffset EffectiveFromUtc,
    DateTimeOffset? EffectiveToUtc,
    OrganizationConfigurationSource Source,
    Guid? BaseConfigurationId = null,
    int? BranchOverrideVersionNumber = null);
