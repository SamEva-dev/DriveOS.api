using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Domain.Sessions.Events;

public sealed record TrainingSessionInterruptedDomainEvent(
    TrainingSessionId SessionId,
    OrganizationId OrganizationId,
    TrainingSessionInterruptionId InterruptionId,
    TrainingSessionInterruptionReason Reason,
    DateTimeOffset InterruptedAtUtc) : DomainEvent;
