using DriveOS.Modules.SchedulingCapacity.Domain.Bookings;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Bookings.Events;

public sealed record BookingCancellationConsequencesRequestedDomainEvent(
    BookingId BookingId,
    OrganizationId OrganizationId,
    BookingCancellationId CancellationId,
    BookingCreditDecision CreditDecision,
    BookingFeeDecision FeeDecision) : DomainEvent;
