using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Domain.OrganizationConfigurations.Events;

public sealed record OrganizationConfigurationPublishedDomainEvent(
    OrganizationConfigurationId ConfigurationId,
    OrganizationId OrganizationId,
    int VersionNumber,
    DateTimeOffset EffectiveFromUtc
) : DomainEvent;
