using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Domain.Sessions.Events;

public sealed record TrainingSessionOdometerRecordedDomainEvent(
    TrainingSessionId SessionId,
    OrganizationId OrganizationId,
    Guid VehicleId,
    TrainingSessionOdometerReadingId ReadingId,
    decimal OdometerKilometers,
    DateTimeOffset ObservedAtUtc) : DomainEvent;
