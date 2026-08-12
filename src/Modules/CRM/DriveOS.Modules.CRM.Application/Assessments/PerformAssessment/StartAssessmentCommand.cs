using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Domain.Assessments;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Application.Assessments.PerformAssessment;

public sealed record StartAssessmentCommand(OrganizationId OrganizationId, AssessmentAppointmentId AppointmentId, UserId EvaluatorUserId,
    string QuestionnaireCode, int QuestionnaireVersion, string QuestionnaireSnapshotJson,
    DateTimeOffset StartedAtUtc) : ICommand<Guid>;
