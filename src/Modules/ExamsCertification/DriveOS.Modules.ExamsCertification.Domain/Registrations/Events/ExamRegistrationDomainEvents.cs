using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Domain.Registrations.Events;

public sealed record ExamRegistrationCreatedDomainEvent(
    ExamRegistrationId RegistrationId,
    OrganizationId OrganizationId,
    PersonId StudentId,
    ExamPlaceId ExamPlaceId,
    ExamReadinessDecisionId ReadinessDecisionId) : DomainEvent;

public sealed record ExamRegistrationPlaceAssignedDomainEvent(
    ExamRegistrationId RegistrationId,
    OrganizationId OrganizationId,
    ExamPlaceId ExamPlaceId) : DomainEvent;
