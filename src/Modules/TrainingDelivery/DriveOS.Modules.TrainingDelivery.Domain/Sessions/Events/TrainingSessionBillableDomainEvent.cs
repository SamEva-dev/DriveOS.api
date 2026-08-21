using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Domain.Sessions.Events;

public sealed record TrainingSessionBillableDomainEvent(
    TrainingSessionId SessionId,
    OrganizationId OrganizationId,
    OrganizationId StudentOwnerOrganizationId,
    OrganizationId PerformingOrganizationId,
    PersonId StudentId,
    string? PricingReference,
    int DeliveredDurationMinutes,
    DateTimeOffset CompletedAtUtc) : DomainEvent;
