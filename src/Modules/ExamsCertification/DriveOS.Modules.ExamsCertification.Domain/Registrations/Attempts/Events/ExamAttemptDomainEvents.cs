using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Domain.Registrations.Attempts.Events;

public sealed record ExamAttemptCreatedDomainEvent(ExamAttemptId AttemptId, OrganizationId OrganizationId, ExamRegistrationId RegistrationId, PersonId StudentId, int AttemptNumber) : DomainEvent;
public sealed record ExamCandidateCheckedInDomainEvent(ExamAttemptId AttemptId, OrganizationId OrganizationId, ExamRegistrationId RegistrationId, DateTimeOffset CheckedInAtUtc) : DomainEvent;
public sealed record ExamAttemptStartedDomainEvent(ExamAttemptId AttemptId, OrganizationId OrganizationId, ExamRegistrationId RegistrationId, DateTimeOffset StartedAtUtc) : DomainEvent;
public sealed record ExamAttemptCompletedDomainEvent(ExamAttemptId AttemptId, OrganizationId OrganizationId, ExamRegistrationId RegistrationId, DateTimeOffset CompletedAtUtc) : DomainEvent;
public sealed record ExamCandidateAbsentDomainEvent(ExamAttemptId AttemptId, OrganizationId OrganizationId, ExamRegistrationId RegistrationId, bool Excused, string ReasonCode) : DomainEvent;
public sealed record ExamAttemptPostponedDomainEvent(ExamAttemptId AttemptId, OrganizationId OrganizationId, ExamRegistrationId RegistrationId, string ReasonCode) : DomainEvent;
public sealed record ExamAttemptCancelledDomainEvent(ExamAttemptId AttemptId, OrganizationId OrganizationId, ExamRegistrationId RegistrationId, string ReasonCode) : DomainEvent;
public sealed record ExamAttemptInterruptedDomainEvent(ExamAttemptId AttemptId, OrganizationId OrganizationId, ExamRegistrationId RegistrationId, string ReasonCode) : DomainEvent;
public sealed record ExamAttemptUnableToStartDomainEvent(ExamAttemptId AttemptId, OrganizationId OrganizationId, ExamRegistrationId RegistrationId, string ReasonCode) : DomainEvent;

public sealed record ExamAttemptIncidentReportedDomainEvent(ExamAttemptId AttemptId, OrganizationId OrganizationId, ExamRegistrationId RegistrationId, string IncidentCode) : DomainEvent;
