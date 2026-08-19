using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.CurriculumPedagogy.Application.Competencies;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.CurriculumPedagogy;

internal static class CompetencyAssessmentEndpoints
{
    internal static RouteGroupBuilder MapCompetencyAssessmentEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/training-paths/{trainingPathId:guid}/competencies", List)
            .RequireAuthorization("Pedagogy.Competencies.Read");
        group.MapGet("/training-paths/{trainingPathId:guid}/competencies/{competencyId:guid}", Get)
            .RequireAuthorization("Pedagogy.Competencies.Read");
        group.MapPost("/training-paths/{trainingPathId:guid}/competencies/{competencyId:guid}/assessments", Record)
            .RequireAuthorization("Pedagogy.Competencies.Assess");
        return group;
    }

    private static IResult Fail(Error error) => Results.Problem(
        statusCode: error.Type == ErrorType.NotFound ? StatusCodes.Status404NotFound :
                    error.Type == ErrorType.Conflict ? StatusCodes.Status409Conflict :
                    StatusCodes.Status400BadRequest,
        title: error.Code,
        detail: error.MessageKey);

    private static async Task<IResult> List(Guid trainingPathId, ICompetencyRecordReadService readService, ICurrentTenant tenant, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId) return Results.Unauthorized();
        return Results.Ok(await readService.ListForTrainingPathAsync(organizationId, new TrainingPathId(trainingPathId), true, ct));
    }

    private static async Task<IResult> Get(Guid trainingPathId, Guid competencyId, ICompetencyRecordReadService readService, ICurrentTenant tenant, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId) return Results.Unauthorized();
        var result = await readService.GetAsync(organizationId, new TrainingPathId(trainingPathId), new CompetencyId(competencyId), true, ct);
        return result is null ? Results.NotFound() : Results.Ok(result);
    }

    private static async Task<IResult> Record(Guid trainingPathId, Guid competencyId, RecordCompetencyAssessmentRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId || user.UserId is not { } assessorUserId) return Results.Unauthorized();
        Result<CompetencyAssessmentId> result = await mediator.Send(new RecordCompetencyAssessmentCommand(
            organizationId,
            new TrainingPathId(trainingPathId),
            new CompetencyId(competencyId),
            request.LevelCode,
            assessorUserId,
            request.SourceSessionId,
            request.Comment,
            request.IsVisibleToStudent,
            request.AssessedAtUtc), ct);
        return result.IsSuccess
            ? Results.Created($"/api/pedagogy/training-paths/{trainingPathId}/competencies/{competencyId}", new { id = result.Value.Value })
            : Fail(result.Error);
    }
}

public sealed record RecordCompetencyAssessmentRequest(
    string LevelCode,
    Guid? SourceSessionId,
    string? Comment,
    bool IsVisibleToStudent,
    DateTimeOffset? AssessedAtUtc);
