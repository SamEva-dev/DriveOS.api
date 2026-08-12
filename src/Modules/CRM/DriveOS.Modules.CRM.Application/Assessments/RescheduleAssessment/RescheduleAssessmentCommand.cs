using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.Modules.CRM.Domain.Assessments;

namespace DriveOS.Modules.CRM.Application.Assessments.RescheduleAssessment;

public sealed record RescheduleAssessmentCommand(
    OrganizationId OrganizationId,
    AssessmentAppointmentId AppointmentId,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc) : ICommand;
