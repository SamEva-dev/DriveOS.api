using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Domain.OrganizationClosures.Events;

public sealed record OrganizationClosureStatusChangedDomainEvent(
    OrganizationClosureId ClosureId,
    OrganizationId OrganizationId,
    OrganizationClosureStatus PreviousStatus,
    OrganizationClosureStatus NewStatus,
    UserId ActorUserId,
    string? Comment) : DomainEvent;
