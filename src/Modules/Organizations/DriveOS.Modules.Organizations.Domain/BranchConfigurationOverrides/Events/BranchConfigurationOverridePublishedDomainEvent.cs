using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Domain.BranchConfigurationOverrides.Events;

public sealed record BranchConfigurationOverridePublishedDomainEvent(
    BranchConfigurationOverrideId OverrideId,
    OrganizationId OrganizationId,
    BranchId BranchId,
    int VersionNumber,
    DateTimeOffset EffectiveFromUtc) : DomainEvent;
