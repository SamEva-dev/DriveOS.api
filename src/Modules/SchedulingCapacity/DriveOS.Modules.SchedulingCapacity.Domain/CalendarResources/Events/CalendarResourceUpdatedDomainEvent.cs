using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.CalendarResources.Events;

public sealed record CalendarResourceUpdatedDomainEvent(
    CalendarResourceId CalendarResourceId,
    OrganizationId OrganizationId) : DomainEvent;
