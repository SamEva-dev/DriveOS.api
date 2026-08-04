using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Domain.OrganizationConfigurations.Events;

public sealed record OrganizationConfigurationCreatedDomainEvent(
    OrganizationConfigurationId ConfigurationId,
    OrganizationId OrganizationId,
    int VersionNumber) : DomainEvent;
