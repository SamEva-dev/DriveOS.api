using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Bookings.Events;

public sealed record BookingAttendanceConsequencesRequestedDomainEvent(
    BookingId BookingId,
    OrganizationId OrganizationId,
    BookingAttendanceId AttendanceId,
    AttendanceChargeDecision ChargeDecision,
    AttendanceCreditDecision CreditDecision) : DomainEvent;
