using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.CalendarResources.Events;

public sealed record CalendarResourceCreatedDomainEvent(
    CalendarResourceId CalendarResourceId,
    OrganizationId OrganizationId,
    CalendarResourceType ResourceType,
    Guid ExternalResourceId) : DomainEvent;
