using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Domain.Remediation.Events;

public sealed record ExamRemediationRequestedDomainEvent(ExamRemediationRequestId RequestId, ExamFailureAnalysisId AnalysisId,
    ExamResultId ResultId, int ResultRevision, ExamAttemptId AttemptId, OrganizationId OrganizationId, PersonId StudentId) : DomainEvent;

public sealed record ExamRemediationPlanProvisionedDomainEvent(ExamRemediationRequestId RequestId, RemediationPlanId PlanId,
    OrganizationId OrganizationId, PersonId StudentId) : DomainEvent;

public sealed record ExamRemediationCompletedDomainEvent(ExamRemediationRequestId RequestId, RemediationPlanId PlanId,
    OrganizationId OrganizationId, PersonId StudentId) : DomainEvent;

public sealed record ExamRemediationValidatedForRePresentationDomainEvent(ExamRemediationRequestId RequestId,
    OrganizationId OrganizationId, PersonId StudentId, ExamAttemptId FailedAttemptId) : DomainEvent;

public sealed record ExamRemediationSupersededDomainEvent(ExamRemediationRequestId RequestId, ExamResultId ResultId,
    int ResultRevision, OrganizationId OrganizationId) : DomainEvent;
