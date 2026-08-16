using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Domain.Assessments;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Application.Assessments.PerformAssessment;

internal sealed class GetAssessmentSessionQueryHandler(IAssessmentSessionRepository sessions)
    : IQueryHandler<GetAssessmentSessionQuery, AssessmentSessionResponse>
{
    public async Task<Result<AssessmentSessionResponse>> Handle(
        GetAssessmentSessionQuery query,
        CancellationToken cancellationToken
    )
    {
        AssessmentSession? session = await sessions.GetByAppointmentAsync(
            query.OrganizationId,
            query.AppointmentId,
            cancellationToken
        );
        if (session is null)
            return Result.Failure<AssessmentSessionResponse>(AssessmentSessionErrors.NotFound);
        return Result.Success(
            new AssessmentSessionResponse(
                session.Id.Value,
                session.AppointmentId.Value,
                session.LeadId.Value,
                session.EvaluatorUserId.Value,
                session.QuestionnaireCode,
                session.QuestionnaireVersion,
                session.QuestionnaireSnapshotJson,
                session.AnswersJson,
                session.FactualObservations,
                session.PedagogicalInterpretation,
                session.Recommendation,
                session.InternalNotes,
                session.ProspectComment,
                session.Status.ToString(),
                session.Revision,
                session.StartedAtUtc,
                session.LastSavedAtUtc,
                session.SubmittedAtUtc,
                session.SubmittedByUserId?.Value,
                session.ResultJson,
                session.AiSuggestionJson,
                session.ResultConfidence?.ToString(),
                session.ResultStatus.ToString(),
                session.CorrectionReason,
                session.ResultValidatedAtUtc,
                session.ResultValidatedByUserId?.Value,
                session.ResultSharedAtUtc,
                session.ResultSharedByUserId?.Value
            )
        );
    }
}
