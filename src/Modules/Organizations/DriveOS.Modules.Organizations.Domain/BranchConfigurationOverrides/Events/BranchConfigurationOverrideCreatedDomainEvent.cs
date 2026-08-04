using DriveOS.Modules.Organizations.Domain.OrganizationConfigurations;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Domain.BranchConfigurationOverrides.Events;

public sealed record BranchConfigurationOverrideCreatedDomainEvent(
    BranchConfigurationOverrideId OverrideId,
    OrganizationId OrganizationId,
    BranchId BranchId,
    OrganizationConfigurationId BaseConfigurationId,
    int VersionNumber) : DomainEvent;
