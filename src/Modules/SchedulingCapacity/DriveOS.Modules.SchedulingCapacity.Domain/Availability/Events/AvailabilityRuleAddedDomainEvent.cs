using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Availability.Events;

public sealed record AvailabilityRuleAddedDomainEvent(
    AvailabilityPlanId AvailabilityPlanId,
    OrganizationId OrganizationId,
    AvailabilityRuleId AvailabilityRuleId,
    DayOfWeek DayOfWeek) : DomainEvent;
