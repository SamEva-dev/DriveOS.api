using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.ExamsCertification.Application.Readiness.Opinions;
using DriveOS.Modules.ExamsCertification.Domain.Readiness.Opinions;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.ExamsCertification;

internal static class ExamReadinessOpinionEndpoints
{
    internal static IEndpointRouteBuilder MapExamReadinessOpinionEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/exams/readiness")
            .WithTags("Exams - Readiness opinions");

        group.MapGet("/students/{studentId:guid}/opinion-context", GetContext)
            .RequireAuthorization("ExamReadiness.Evaluate");

        group.MapGet("/students/{studentId:guid}/opinions", GetOpinions)
            .RequireAuthorization("ExamReadiness.ReadOpinions");

        group.MapPost("/students/{studentId:guid}/opinions", SubmitOpinion)
            .RequireAuthorization("ExamReadiness.SubmitOpinion");

        return app;
    }

    private static async Task<IResult> GetContext(
        Guid studentId,
        Guid trainingPathId,
        IMediator mediator,
        ICurrentTenant tenant,
        CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId)
            return Results.Unauthorized();

        Result<ExamReadinessOpinionContext> result = await mediator.Send(
            new GetExamReadinessOpinionContextQuery(organizationId, new PersonId(studentId), new TrainingPathId(trainingPathId)),
            cancellationToken);

        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> GetOpinions(
        Guid studentId,
        Guid trainingPathId,
        IMediator mediator,
        ICurrentTenant tenant,
        CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId)
            return Results.Unauthorized();

        Result<IReadOnlyList<ExamReadinessOpinionResponse>> result = await mediator.Send(
            new GetExamReadinessOpinionsQuery(organizationId, new PersonId(studentId), new TrainingPathId(trainingPathId)),
            cancellationToken);

        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> SubmitOpinion(
        Guid studentId,
        SubmitExamReadinessOpinionRequest request,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } authorId)
            return Results.Unauthorized();

        if (!Enum.TryParse(request.Opinion, true, out ExamReadinessOpinionType opinion)
            || !Enum.TryParse(request.ObservedAutonomy, true, out ObservedAutonomyLevel autonomy))
        {
            return Results.BadRequest(new
            {
                code = "Exams.Readiness.Opinion.InvalidEnum",
                messageKey = "errors.exams.readiness.opinion.invalidEnum"
            });
        }

        var reservationCodes = new List<ExamReadinessReservationCode>();
        foreach (string raw in request.ReservationCodes ?? Array.Empty<string>())
        {
            if (!Enum.TryParse(raw, true, out ExamReadinessReservationCode parsed))
            {
                return Results.BadRequest(new
                {
                    code = "Exams.Readiness.Opinion.InvalidReservationCode",
                    messageKey = "errors.exams.readiness.opinion.invalidReservationCode"
                });
            }
            reservationCodes.Add(parsed);
        }

        Result<ExamReadinessOpinionId> result = await mediator.Send(
            new SubmitExamReadinessOpinionCommand(
                organizationId,
                new PersonId(studentId),
                new TrainingPathId(request.TrainingPathId),
                opinion,
                autonomy,
                reservationCodes,
                request.Reservations,
                request.Conditions,
                request.Comment,
                request.OperationId,
                authorId),
            cancellationToken);

        return result.IsSuccess
            ? Results.Created($"/api/exams/readiness/students/{studentId}/opinions?trainingPathId={request.TrainingPathId}", new { id = result.Value.Value })
            : ToProblem(result.Error);
    }

    private static IResult ToProblem(Error error) => Results.Problem(
        statusCode: error.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status400BadRequest
        },
        title: error.Code,
        detail: error.MessageKey);
}

public sealed record SubmitExamReadinessOpinionRequest(
    Guid TrainingPathId,
    string Opinion,
    string ObservedAutonomy,
    IReadOnlyCollection<string>? ReservationCodes,
    string? Reservations,
    string? Conditions,
    string? Comment,
    Guid OperationId);
