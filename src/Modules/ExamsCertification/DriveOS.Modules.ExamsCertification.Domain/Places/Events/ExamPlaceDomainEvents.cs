using DriveOS.Modules.ExamsCertification.Domain.Places;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Domain.Places.Events;

public sealed record ExamPlaceCreatedDomainEvent(ExamPlaceId ExamPlaceId, OrganizationId OrganizationId, ExamCenterId ExamCenterId) : DomainEvent;
public sealed record ExamPlaceHeldDomainEvent(ExamPlaceId ExamPlaceId, OrganizationId OrganizationId, Guid HoldToken, DateTimeOffset ExpiresAtUtc) : DomainEvent;
public sealed record ExamPlaceAssignedDomainEvent(ExamPlaceId ExamPlaceId, OrganizationId OrganizationId, PersonId StudentId, ExamRegistrationId RegistrationId) : DomainEvent;
public sealed record ExamPlaceAvailabilityChangedDomainEvent(ExamPlaceId ExamPlaceId, OrganizationId OrganizationId, ExamPlaceStatus Status, DateTimeOffset ObservedAtUtc) : DomainEvent;
