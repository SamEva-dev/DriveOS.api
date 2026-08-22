using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.ExamsCertification.Application.Readiness;
using DriveOS.Modules.ExamsCertification.Domain.Readiness;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.ExamsCertification;

internal static class ExamReadinessEndpoints
{
    internal static IEndpointRouteBuilder MapExamReadinessEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/exams/readiness")
            .WithTags("Exams - Readiness");

        group.MapGet("/students/{studentId:guid}", GetCurrent)
            .RequireAuthorization("ExamReadiness.Read");

        group.MapGet("/students/{studentId:guid}/snapshot", GetSnapshot)
            .RequireAuthorization("ExamReadiness.Read");

        group.MapPost("/students/{studentId:guid}/decisions", RecordDecision)
            .RequireAuthorization("ExamReadiness.Decide");

        return app;
    }

    private static async Task<IResult> GetCurrent(
        Guid studentId,
        Guid trainingPathId,
        IMediator mediator,
        ICurrentTenant tenant,
        CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId)
            return Results.Unauthorized();

        Result<ExamReadinessDecisionResponse> result = await mediator.Send(
            new GetExamReadinessDecisionQuery(
                organizationId,
                new PersonId(studentId),
                new TrainingPathId(trainingPathId)),
            cancellationToken);

        if (result.IsFailure)
            return ToProblem(result.Error);

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetSnapshot(
        Guid studentId,
        Guid trainingPathId,
        IMediator mediator,
        ICurrentTenant tenant,
        CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId)
            return Results.Unauthorized();

        Result<ExamReadinessSnapshot> result = await mediator.Send(
            new GetExamReadinessSnapshotQuery(
                organizationId,
                new PersonId(studentId),
                new TrainingPathId(trainingPathId)),
            cancellationToken);

        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> RecordDecision(
        Guid studentId,
        RecordExamReadinessDecisionRequest request,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } reviewerId)
            return Results.Unauthorized();

        if (!Enum.TryParse(request.Outcome, true, out ExamReadinessOutcome outcome))
        {
            return Results.BadRequest(new
            {
                code = "Exams.Readiness.Decision.InvalidEnum",
                messageKey = "errors.exams.readiness.decision.invalidEnum"
            });
        }

        Result<ExamReadinessDecisionId> result = await mediator.Send(
            new RecordExamReadinessDecisionCommand(
                organizationId,
                new PersonId(studentId),
                new TrainingPathId(request.TrainingPathId),
                outcome,
                request.Rationale,
                request.Conditions,
                reviewerId),
            cancellationToken);

        return result.IsSuccess
            ? Results.Created(
                $"/api/exams/readiness/students/{studentId}?trainingPathId={request.TrainingPathId}",
                new { id = result.Value.Value })
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

public sealed record RecordExamReadinessDecisionRequest(
    Guid TrainingPathId,
    string Outcome,
    string Rationale,
    string? Conditions);
