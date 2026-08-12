using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.Modules.CRM.Domain.Assessments;

namespace DriveOS.Modules.CRM.Application.Assessments.CancelAssessment;

public sealed record CancelAssessmentCommand(
    OrganizationId OrganizationId,
    AssessmentAppointmentId AppointmentId,
    DateTimeOffset CancelledAtUtc) : ICommand;
