using DriveOS.Modules.SchedulingCapacity.Domain.Bookings;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Bookings.Events;

public sealed record BookingCancelledDomainEvent(
    BookingId BookingId,
    OrganizationId OrganizationId,
    BookingCancellationId CancellationId,
    Guid OperationId,
    CancellationInitiator Initiator,
    CancellationReasonCode ReasonCode,
    string? ReasonDetails,
    DateTimeOffset CancelledAtUtc) : DomainEvent;
