using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Domain.Assessments;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Application.Assessments.PerformAssessment;

public sealed record GetAssessmentSessionQuery(OrganizationId OrganizationId, AssessmentAppointmentId AppointmentId) : IQuery<AssessmentSessionResponse>;
