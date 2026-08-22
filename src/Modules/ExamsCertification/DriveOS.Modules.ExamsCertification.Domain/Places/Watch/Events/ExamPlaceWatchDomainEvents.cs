using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Domain.Places.Watch.Events;

public sealed record ExamPlaceWatchSubscriptionCreatedDomainEvent(
    ExamPlaceWatchSubscriptionId SubscriptionId,
    OrganizationId OrganizationId,
    string ProviderCode) : DomainEvent;

public sealed record ExamPlaceAvailabilityDetectedDomainEvent(
    ExamPlaceWatchSubscriptionId SubscriptionId,
    OrganizationId OrganizationId,
    ExamPlaceId ExamPlaceId,
    DateTimeOffset DetectedAtUtc) : DomainEvent;
