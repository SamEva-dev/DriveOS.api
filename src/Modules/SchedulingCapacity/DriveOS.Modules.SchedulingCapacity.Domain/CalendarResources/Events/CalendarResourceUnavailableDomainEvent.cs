using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.CalendarResources.Events;

public sealed record CalendarResourceUnavailableDomainEvent(
    CalendarResourceId CalendarResourceId,
    OrganizationId OrganizationId,
    string? Reason) : DomainEvent;
