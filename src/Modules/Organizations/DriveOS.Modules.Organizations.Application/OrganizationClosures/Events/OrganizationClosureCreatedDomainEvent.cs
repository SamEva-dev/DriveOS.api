using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Domain.OrganizationClosures.Events;

public sealed record OrganizationClosureCreatedDomainEvent(
    OrganizationClosureId ClosureId,
    OrganizationId OrganizationId,
    OrganizationClosureReasonCode ReasonCode,
    DateTimeOffset RequestedEffectiveAtUtc
) : DomainEvent;
