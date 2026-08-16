using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Domain.Assessments;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Application.Assessments.PerformAssessment;

public sealed record SaveAssessmentDraftCommand(
    OrganizationId OrganizationId,
    AssessmentAppointmentId AppointmentId,
    UserId SavedByUserId,
    string AnswersJson,
    string? FactualObservations,
    string? PedagogicalInterpretation,
    string? Recommendation,
    string? InternalNotes,
    string? ProspectComment,
    bool DraftCompleted,
    DateTimeOffset SavedAtUtc
) : ICommand;
