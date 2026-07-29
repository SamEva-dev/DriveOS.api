using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Domain.Organizations.Events;

public sealed record OrganizationStatusChangedDomainEvent(
    OrganizationId OrganizationId,
    OrganizationStatus PreviousStatus,
    OrganizationStatus NewStatus,
    string Reason,
    Guid ChangedByUserId,
    DateTimeOffset ChangedAtUtc)
    : DomainEvent;