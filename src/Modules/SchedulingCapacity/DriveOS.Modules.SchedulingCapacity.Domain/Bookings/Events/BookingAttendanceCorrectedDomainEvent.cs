using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Bookings.Events;

public sealed record BookingAttendanceCorrectedDomainEvent(
    BookingId BookingId,
    OrganizationId OrganizationId,
    BookingAttendanceId PreviousAttendanceId,
    BookingAttendanceId AttendanceId,
    AttendanceStatus Status,
    bool OverrideApplied) : DomainEvent;
