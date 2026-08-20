using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Availability.Events;

public sealed record AvailabilityExceptionAddedDomainEvent(
    AvailabilityPlanId AvailabilityPlanId,
    OrganizationId OrganizationId,
    AvailabilityExceptionId AvailabilityExceptionId,
    DateOnly Date,
    AvailabilityExceptionType Type) : DomainEvent;
