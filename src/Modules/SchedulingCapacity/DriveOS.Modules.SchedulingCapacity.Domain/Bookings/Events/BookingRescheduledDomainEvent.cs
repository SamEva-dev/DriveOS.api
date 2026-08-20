using DriveOS.Modules.SchedulingCapacity.Domain.Bookings;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Bookings.Events;

public sealed record BookingRescheduledDomainEvent(
    BookingId BookingId,
    OrganizationId OrganizationId,
    Guid OperationId,
    DateTimeOffset PreviousStartAtUtc,
    DateTimeOffset PreviousEndAtUtc,
    DateTimeOffset NewStartAtUtc,
    DateTimeOffset NewEndAtUtc,
    BranchId? PreviousBranchId,
    BranchId? NewBranchId,
    BookingStatus PreviousStatus,
    string Reason,
    bool ResourcesChanged) : DomainEvent;
