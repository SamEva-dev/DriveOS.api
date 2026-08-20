using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Availability.Events;

public sealed record AvailabilityPlanActivatedDomainEvent(
    AvailabilityPlanId AvailabilityPlanId,
    OrganizationId OrganizationId,
    CalendarResourceId CalendarResourceId) : DomainEvent;
