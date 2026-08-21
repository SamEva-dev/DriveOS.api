using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Domain.Sessions.Events;

public sealed record TrainingSessionCompletedDomainEvent(
    TrainingSessionId SessionId,
    OrganizationId OrganizationId,
    OrganizationId StudentOwnerOrganizationId,
    OrganizationId PerformingOrganizationId,
    BookingId SourceBookingId,
    PersonId StudentId,
    TrainingPathId TrainingPathId,
    UserId InstructorId,
    Guid? VehicleId,
    DateTimeOffset ActualStartAtUtc,
    DateTimeOffset ActualEndAtUtc,
    int DeliveredDurationMinutes,
    decimal? DistanceKilometers) : DomainEvent;
