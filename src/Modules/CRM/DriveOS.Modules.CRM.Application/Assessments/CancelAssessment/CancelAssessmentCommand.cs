using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Domain.Assessments;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Application.Assessments.CancelAssessment;

public sealed record CancelAssessmentCommand(
    OrganizationId OrganizationId,
    AssessmentAppointmentId AppointmentId,
    DateTimeOffset CancelledAtUtc
) : ICommand;
