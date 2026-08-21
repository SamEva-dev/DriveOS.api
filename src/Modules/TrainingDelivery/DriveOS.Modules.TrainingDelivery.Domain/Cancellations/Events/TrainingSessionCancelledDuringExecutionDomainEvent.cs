using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Domain.Cancellations.Events;

public sealed record TrainingSessionCancelledDuringExecutionDomainEvent(
    TrainingSessionId SessionId,
    SessionCancellationId CancellationId,
    OrganizationId OrganizationId,
    PersonId StudentId,
    SessionCancellationReason Reason,
    DateTimeOffset ActualEndAtUtc,
    int DeliveredDurationMinutes,
    UserId CancelledByUserId) : DomainEvent;
