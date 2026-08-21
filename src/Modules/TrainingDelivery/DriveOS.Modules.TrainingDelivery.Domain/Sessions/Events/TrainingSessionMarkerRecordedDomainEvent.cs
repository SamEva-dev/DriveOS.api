using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Domain.Sessions.Events;

public sealed record TrainingSessionMarkerRecordedDomainEvent(
    TrainingSessionId SessionId,
    OrganizationId OrganizationId,
    TrainingSessionMarkerId MarkerId,
    TrainingSessionMarkerType Type,
    DateTimeOffset TrainingSessionMarkerRecordedAtUtc) : DomainEvent;
