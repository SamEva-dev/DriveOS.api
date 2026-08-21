using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Domain.Sessions.Events;

public sealed record TrainingSessionResumedDomainEvent(
    TrainingSessionId SessionId,
    OrganizationId OrganizationId,
    TrainingSessionInterruptionId InterruptionId,
    DateTimeOffset ResumedAtUtc) : DomainEvent;
