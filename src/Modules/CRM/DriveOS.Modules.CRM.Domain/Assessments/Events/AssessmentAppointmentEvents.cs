using DriveOS.SharedKernel.Domain;

namespace DriveOS.Modules.CRM.Domain.Assessments.Events;

public sealed record AssessmentAppointmentScheduledDomainEvent(
    AssessmentAppointmentId AssessmentAppointmentId, OrganizationId OrganizationId,
    LeadId LeadId, BranchId? BranchId, DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc) : DomainEvent;

public sealed record AssessmentAppointmentRescheduledDomainEvent(
    AssessmentAppointmentId AssessmentAppointmentId, OrganizationId OrganizationId,
    DateTimeOffset StartsAtUtc, DateTimeOffset EndsAtUtc) : DomainEvent;

public sealed record AssessmentAppointmentCancelledDomainEvent(
    AssessmentAppointmentId AssessmentAppointmentId, OrganizationId OrganizationId,
    DateTimeOffset CancelledAtUtc) : DomainEvent;

public sealed record AssessmentAppointmentCompletedDomainEvent(
    AssessmentAppointmentId AssessmentAppointmentId, OrganizationId OrganizationId,
    DateTimeOffset CompletedAtUtc) : DomainEvent;
