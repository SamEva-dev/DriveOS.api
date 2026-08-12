using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Domain.Assessments;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Application.Assessments.PerformAssessment;

public sealed record SubmitAssessmentCommand(OrganizationId OrganizationId, AssessmentAppointmentId AppointmentId,
    UserId SubmittedByUserId, DateTimeOffset SubmittedAtUtc) : ICommand;
