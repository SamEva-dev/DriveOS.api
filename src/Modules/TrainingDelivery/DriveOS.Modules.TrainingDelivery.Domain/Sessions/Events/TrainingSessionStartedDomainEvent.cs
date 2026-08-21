using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Domain.Sessions.Events;

public sealed record TrainingSessionStartedDomainEvent(
    TrainingSessionId SessionId,
    OrganizationId OrganizationId,
    BookingId SourceBookingId,
    PersonId StudentId,
    UserId InstructorId,
    Guid? VehicleId,
    DateTimeOffset ActualStartAtUtc) : DomainEvent;
