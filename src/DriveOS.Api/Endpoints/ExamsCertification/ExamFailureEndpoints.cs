using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.ExamsCertification.Application.Results.Failure;
using DriveOS.Security.Contracts;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.ExamsCertification;

internal static class ExamFailureEndpoints
{
    internal static IEndpointRouteBuilder MapExamFailureEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/exams/results/{resultId:guid}/failure-analysis").WithTags("Exams & Certification");
        group.MapGet("/", Get).RequireAuthorization(DriveOsPermissionCodes.Exams.FailureAnalysisRead);
        group.MapPost("/{revision:int}/findings", AddFinding).RequireAuthorization(DriveOsPermissionCodes.Exams.FailureAnalysisManage);
        group.MapPut("/{revision:int}/narrative", UpdateNarrative).RequireAuthorization(DriveOsPermissionCodes.Exams.FailureAnalysisManage);
        group.MapPost("/{revision:int}/complete", Complete).RequireAuthorization(DriveOsPermissionCodes.Exams.FailureAnalysisManage);
        return app;
    }

    private static async Task<IResult> Get(Guid resultId, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId) return Results.Unauthorized();
        Result<ExamFailureAnalysisResponse> result = await mediator.Send(new GetExamFailureAnalysisQuery(organizationId, new ExamResultId(resultId)), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private sealed record AddFindingRequest(string Kind, string Code, string? Detail, bool Critical, string Source);
    private static async Task<IResult> AddFinding(Guid resultId, int revision, AddFindingRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId || user.UserId is not { } userId) return Results.Unauthorized();
        Result<ExamFailureAnalysisResponse> result = await mediator.Send(new AddExamFailureFindingCommand(organizationId, new ExamResultId(resultId), revision,
            request.Kind, request.Code, request.Detail, request.Critical, request.Source, userId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private sealed record UpdateNarrativeRequest(string? InstructorAnalysis, string? StudentFeedback, string? Recommendation);
    private static async Task<IResult> UpdateNarrative(Guid resultId, int revision, UpdateNarrativeRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId || user.UserId is not { } userId) return Results.Unauthorized();
        Result<ExamFailureAnalysisResponse> result = await mediator.Send(new UpdateExamFailureNarrativeCommand(organizationId, new ExamResultId(resultId), revision,
            request.InstructorAnalysis, request.StudentFeedback, request.Recommendation, userId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private sealed record CompleteRequest(string Summary, string? Recommendation);
    private static async Task<IResult> Complete(Guid resultId, int revision, CompleteRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId || user.UserId is not { } userId) return Results.Unauthorized();
        Result<ExamFailureAnalysisResponse> result = await mediator.Send(new CompleteExamFailureAnalysisCommand(organizationId, new ExamResultId(resultId), revision,
            request.Summary, request.Recommendation, userId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private static IResult Failure(Error e) => Results.Problem(statusCode: e.Type switch { ErrorType.NotFound => 404, ErrorType.Conflict => 409, ErrorType.Validation => 400, _ => 400 }, title: e.Code,
        extensions: new Dictionary<string, object?> { ["code"] = e.Code, ["messageKey"] = e.MessageKey });
}
