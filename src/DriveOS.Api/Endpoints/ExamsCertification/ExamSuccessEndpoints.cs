using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.ExamsCertification.Application.Results.Success;
using DriveOS.Modules.ExamsCertification.Application.Success;
using DriveOS.Security.Contracts;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.ExamsCertification;

internal static class ExamSuccessEndpoints
{
    internal static IEndpointRouteBuilder MapExamSuccessEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/exams/results/{resultId:guid}/success").WithTags("Exams & Certification");
        group.MapGet("/process", GetProcess).RequireAuthorization(DriveOsPermissionCodes.Exams.SuccessRead);
        group.MapPost("/process/{revision:int}/complete", CompleteProcess).RequireAuthorization(DriveOsPermissionCodes.Exams.SuccessManage);
        group.MapPost("/process/{revision:int}/archive", ArchiveProcess).RequireAuthorization(DriveOsPermissionCodes.Exams.SuccessManage);
        group.MapGet("/consequences", GetConsequences).RequireAuthorization(DriveOsPermissionCodes.Exams.SuccessRead);
        group.MapPost("/consequences/requeue", Requeue).RequireAuthorization(DriveOsPermissionCodes.Exams.SuccessManage);
        return app;
    }

    private static async Task<IResult> GetProcess(Guid resultId, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId) return Results.Unauthorized();
        Result<ExamSuccessProcessResponse> result = await mediator.Send(new GetExamSuccessProcessQuery(organizationId, new ExamResultId(resultId)), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private static async Task<IResult> CompleteProcess(Guid resultId, int revision, IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } userId) return Results.Unauthorized();
        Result<ExamSuccessProcessResponse> result = await mediator.Send(new CompleteExamSuccessProcessCommand(organizationId, new ExamResultId(resultId), revision, userId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private static async Task<IResult> ArchiveProcess(Guid resultId, int revision, IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } userId) return Results.Unauthorized();
        Result<ExamSuccessProcessResponse> result = await mediator.Send(new ArchiveExamSuccessProcessCommand(organizationId, new ExamResultId(resultId), revision, userId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private static async Task<IResult> GetConsequences(Guid resultId, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId) return Results.Unauthorized();
        Result<IReadOnlyList<ExamSuccessConsequenceResponse>> result = await mediator.Send(new GetExamSuccessConsequencesQuery(organizationId, new ExamResultId(resultId)), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private static async Task<IResult> Requeue(Guid resultId, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId) return Results.Unauthorized();
        Result<IReadOnlyList<ExamSuccessConsequenceResponse>> result = await mediator.Send(new RequeueExamSuccessConsequencesCommand(organizationId, new ExamResultId(resultId)), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private static IResult Failure(Error e) => Results.Problem(statusCode: e.Type switch { ErrorType.NotFound => 404, ErrorType.Conflict => 409, ErrorType.Validation => 400, _ => 400 }, title: e.Code,
        extensions: new Dictionary<string, object?> { ["code"] = e.Code, ["messageKey"] = e.MessageKey });
}
