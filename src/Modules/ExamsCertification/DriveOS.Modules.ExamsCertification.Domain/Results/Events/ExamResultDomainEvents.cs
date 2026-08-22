using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Domain.Results.Events;

public sealed record ExamResultRecordedDomainEvent(ExamResultId ResultId, ExamAttemptId AttemptId, OrganizationId OrganizationId, ExamResultOutcome Outcome, int Revision) : DomainEvent;
public sealed record ExamResultVerifiedDomainEvent(ExamResultId ResultId, ExamAttemptId AttemptId, OrganizationId OrganizationId, int Revision) : DomainEvent;
public sealed record ExamResultFinalizedDomainEvent(ExamResultId ResultId, ExamAttemptId AttemptId, OrganizationId OrganizationId, ExamResultOutcome Outcome, int Revision) : DomainEvent;
public sealed record ExamResultFinalizationSupersededDomainEvent(ExamResultId ResultId, ExamAttemptId AttemptId, OrganizationId OrganizationId, ExamResultOutcome PreviousOutcome, int PreviousRevision) : DomainEvent;
public sealed record ExamResultCorrectedDomainEvent(ExamResultId ResultId, ExamAttemptId AttemptId, OrganizationId OrganizationId, int PreviousRevision, int NewRevision, ExamResultOutcome Outcome) : DomainEvent;
public sealed record ExamPassedDomainEvent(ExamResultId ResultId, ExamAttemptId AttemptId, OrganizationId OrganizationId, PersonId StudentId, int AttemptNumber) : DomainEvent;
public sealed record ExamFailedDomainEvent(ExamResultId ResultId, ExamAttemptId AttemptId, OrganizationId OrganizationId, PersonId StudentId, int AttemptNumber) : DomainEvent;
