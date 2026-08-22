using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Domain.Results.Failure.Events;

public sealed record ExamFailureAnalysisCreatedDomainEvent(ExamFailureAnalysisId AnalysisId, ExamResultId ResultId, int ResultRevision,
    ExamAttemptId AttemptId, OrganizationId OrganizationId, PersonId StudentId) : DomainEvent;
public sealed record ExamFailureFindingAddedDomainEvent(ExamFailureAnalysisId AnalysisId, ExamFailureFindingKind Kind, string Code) : DomainEvent;
public sealed record ExamFailureAnalysisSubmittedDomainEvent(ExamFailureAnalysisId AnalysisId, ExamResultId ResultId, int ResultRevision,
    ExamAttemptId AttemptId, OrganizationId OrganizationId, PersonId StudentId) : DomainEvent;
public sealed record ExamFailureAnalysisApprovedDomainEvent(ExamFailureAnalysisId AnalysisId, ExamResultId ResultRevisionId, int ResultRevision,
    ExamAttemptId AttemptId, OrganizationId OrganizationId, PersonId StudentId, int AttemptNumber) : DomainEvent;
public sealed record ExamFailureAnalysisCompletedDomainEvent(ExamFailureAnalysisId AnalysisId, ExamResultId ResultId, int ResultRevision,
    ExamAttemptId AttemptId, OrganizationId OrganizationId, PersonId StudentId, int AttemptNumber) : DomainEvent;
public sealed record ExamFailureAnalysisSupersededDomainEvent(ExamFailureAnalysisId AnalysisId, ExamResultId ResultId, int ResultRevision,
    OrganizationId OrganizationId) : DomainEvent;
