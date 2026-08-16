using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Domain.Assessments;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Application.Assessments.RescheduleAssessment;

public sealed record RescheduleAssessmentCommand(
    OrganizationId OrganizationId,
    AssessmentAppointmentId AppointmentId,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc
) : ICommand;
