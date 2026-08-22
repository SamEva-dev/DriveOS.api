using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.ExamsCertification.Domain.Results.Success.Events;
public sealed record ExamSuccessProcessStartedDomainEvent(ExamSuccessProcessId SuccessProcessId, OrganizationId OrganizationId, ExamResultId ResultId, int ResultRevision, ExamAttemptId AttemptId, ExamRegistrationId RegistrationId, PersonId StudentId) : DomainEvent;
public sealed record ExamSuccessActionStateChangedDomainEvent(ExamSuccessProcessId SuccessProcessId, ExamResultId ResultId, ExamSuccessActionCode ActionCode, ExamSuccessActionStatus Status) : DomainEvent;
public sealed record ExamSuccessProcessCompletedDomainEvent(ExamSuccessProcessId SuccessProcessId, OrganizationId OrganizationId, ExamResultId ResultId, PersonId StudentId) : DomainEvent;
public sealed record ExamSuccessProcessSupersededDomainEvent(ExamSuccessProcessId SuccessProcessId, OrganizationId OrganizationId, ExamResultId ResultId, int ResultRevision) : DomainEvent;
