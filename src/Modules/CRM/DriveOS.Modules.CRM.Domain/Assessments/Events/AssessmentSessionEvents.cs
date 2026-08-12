using DriveOS.SharedKernel.Domain;

namespace DriveOS.Modules.CRM.Domain.Assessments.Events;

public sealed record InitialAssessmentStartedDomainEvent(AssessmentSessionId SessionId, AssessmentAppointmentId AppointmentId, OrganizationId OrganizationId, UserId EvaluatorUserId, DateTimeOffset StartedAtUtc) : DomainEvent;
public sealed record InitialAssessmentDraftSavedDomainEvent(AssessmentSessionId SessionId, OrganizationId OrganizationId, int Revision, DateTimeOffset SavedAtUtc) : DomainEvent;
public sealed record InitialAssessmentSubmittedDomainEvent(AssessmentSessionId SessionId, AssessmentAppointmentId AppointmentId, OrganizationId OrganizationId, UserId SubmittedByUserId, int Revision, DateTimeOffset SubmittedAtUtc) : DomainEvent;
public sealed record InitialAssessmentResultDraftSavedDomainEvent(AssessmentSessionId SessionId, OrganizationId OrganizationId, UserId SavedByUserId, int Revision, DateTimeOffset SavedAtUtc) : DomainEvent;
public sealed record InitialAssessmentResultCorrectionRequestedDomainEvent(AssessmentSessionId SessionId, OrganizationId OrganizationId, UserId RequestedByUserId, int Revision, DateTimeOffset RequestedAtUtc) : DomainEvent;
public sealed record InitialAssessmentResultValidatedDomainEvent(AssessmentSessionId SessionId, AssessmentAppointmentId AppointmentId, LeadId LeadId, OrganizationId OrganizationId, UserId ValidatedByUserId, int Revision, DateTimeOffset ValidatedAtUtc) : DomainEvent;
public sealed record InitialAssessmentResultSharedDomainEvent(AssessmentSessionId SessionId, LeadId LeadId, OrganizationId OrganizationId, UserId SharedByUserId, int Revision, DateTimeOffset SharedAtUtc) : DomainEvent;
