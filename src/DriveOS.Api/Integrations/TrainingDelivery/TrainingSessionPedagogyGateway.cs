using DomainRelay.Abstractions;
using DriveOS.Modules.CurriculumPedagogy.Application.Competencies;
using DriveOS.Modules.CurriculumPedagogy.Application.TrainingPaths;
using DriveOS.Modules.TrainingDelivery.Application.Sessions;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Integrations.TrainingDelivery;

internal sealed class TrainingSessionPedagogyGateway(
    IMediator mediator,
    ITrainingPathReadService trainingPaths) : ITrainingSessionPedagogyGateway
{
    public async Task<Result<TrainingSessionPedagogyAssessmentReference>> RecordAssessmentAsync(
        TrainingSessionPedagogyAssessmentRequest request,
        CancellationToken cancellationToken = default)
    {
        TrainingPathDetailResponse? path = await trainingPaths.GetAsync(
            request.OrganizationId,
            request.TrainingPathId,
            cancellationToken);

        if (path is null)
            return Result.Failure<TrainingSessionPedagogyAssessmentReference>(RecordCompetencyAssessmentErrors.TrainingPathNotFound);

        string? pedagogyComment = !string.IsNullOrWhiteSpace(request.SharedComment)
            ? request.SharedComment
            : request.InternalComment;
        bool visibleToStudent = !string.IsNullOrWhiteSpace(request.SharedComment);

        Result<DriveOS.SharedKernel.Identifiers.CompetencyAssessmentId> recorded = await mediator.Send(
            new RecordCompetencyAssessmentCommand(
                request.OrganizationId,
                request.TrainingPathId,
                request.CompetencyId,
                request.LevelCode,
                request.AssessorUserId,
                request.SessionId.Value,
                pedagogyComment,
                visibleToStudent,
                request.AssessedAtUtc),
            cancellationToken);

        return recorded.IsSuccess
            ? Result.Success(new TrainingSessionPedagogyAssessmentReference(
                recorded.Value.Value,
                new DriveOS.SharedKernel.Identifiers.CurriculumVersionId(path.CurriculumVersionId)))
            : Result.Failure<TrainingSessionPedagogyAssessmentReference>(recorded.Error);
    }
}
