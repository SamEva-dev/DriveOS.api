using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Bookings.Events;

public sealed record BookingInstructorReplacedDomainEvent(
    BookingId BookingId,
    OrganizationId OrganizationId,
    Guid OperationId,
    UserId PreviousInstructorId,
    UserId ReplacementInstructorId,
    DateTimeOffset? AccessExpiresAtUtc) : DomainEvent;
