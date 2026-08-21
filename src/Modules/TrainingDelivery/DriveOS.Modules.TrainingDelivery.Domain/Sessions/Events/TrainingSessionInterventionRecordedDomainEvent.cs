using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Domain.Sessions.Events;

public sealed record TrainingSessionInterventionRecordedDomainEvent(
    TrainingSessionId SessionId,
    OrganizationId OrganizationId,
    TrainingSessionInterventionId InterventionId,
    TrainingSessionInterventionType Type,
    TrainingSessionInterventionSeverity Severity,
    DateTimeOffset TrainingSessionInterventionRecordedAtUtc) : DomainEvent;
