using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Domain.Readiness.Events;

public sealed record ExamReadinessDecisionRecordedDomainEvent(
    ExamReadinessDecisionId DecisionId,
    OrganizationId OrganizationId,
    PersonId StudentId,
    TrainingPathId TrainingPathId,
    int Version,
    ExamReadinessOutcome Outcome,
    UserId ReviewerId) : DomainEvent;

public sealed record ExamReadinessDecisionSupersededDomainEvent(
    ExamReadinessDecisionId PreviousDecisionId,
    ExamReadinessDecisionId NewDecisionId,
    OrganizationId OrganizationId,
    PersonId StudentId) : DomainEvent;

public sealed record StudentMarkedExamReadyDomainEvent(
    ExamReadinessDecisionId DecisionId,
    OrganizationId OrganizationId,
    PersonId StudentId,
    TrainingPathId TrainingPathId,
    UserId ReviewerId) : DomainEvent;
