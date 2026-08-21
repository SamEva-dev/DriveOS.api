using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Domain.Sessions.Events;

public sealed record TrainingSessionReadyDomainEvent(
    TrainingSessionId SessionId,
    OrganizationId OrganizationId,
    BookingId SourceBookingId,
    UserId InstructorId,
    Guid? VehicleId,
    DateTimeOffset CheckedAtUtc) : DomainEvent;
