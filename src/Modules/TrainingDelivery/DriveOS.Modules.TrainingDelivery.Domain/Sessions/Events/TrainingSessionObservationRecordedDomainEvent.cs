using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Domain.Sessions.Events;

public sealed record TrainingSessionObservationRecordedDomainEvent(
    TrainingSessionId SessionId,
    OrganizationId OrganizationId,
    TrainingSessionObservationId ObservationId,
    TrainingSessionObservationType Type,
    DateTimeOffset ObservedAtUtc) : DomainEvent;
