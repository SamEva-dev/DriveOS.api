using System.Text.Json;
using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.ExamsCertification.Application.Registrations;
using DriveOS.Modules.ExamsCertification.Application.Registrations.File;
using DriveOS.Modules.ExamsCertification.Application.Registrations.Submissions;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.ExamsCertification;

internal static class ExamRegistrationEndpoints
{
    internal static IEndpointRouteBuilder MapExamRegistrationEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/exams").WithTags("Exams - Registrations");
        group.MapPost("/places/{placeId:guid}/hold", HoldPlace).RequireAuthorization("ExamRegistrations.Create");
        group.MapPost("/places/{placeId:guid}/hold/release", ReleasePlace).RequireAuthorization("ExamRegistrations.Create");
        group.MapPost("/registrations", CreateRegistration).RequireAuthorization("ExamRegistrations.Create");
        group.MapGet("/registrations/{registrationId:guid}", GetRegistration).RequireAuthorization("ExamRegistrations.Read");
        group.MapGet("/students/{studentId:guid}/registrations", GetStudentRegistrations).RequireAuthorization("ExamRegistrations.Read");
        group.MapGet("/registrations/{registrationId:guid}/file", GetRegistrationFile).RequireAuthorization("ExamRegistrations.Read");
        group.MapGet("/registrations/{registrationId:guid}/file/export", ExportRegistrationFile).RequireAuthorization("ExamRegistrations.Read");
        group.MapPost("/registrations/{registrationId:guid}/file/refresh", RefreshRegistrationFile).RequireAuthorization("ExamRegistrations.Update");
        group.MapPut("/registrations/{registrationId:guid}/file/official-data", UpdateOfficialData).RequireAuthorization("ExamRegistrations.Update");
        group.MapPost("/registrations/{registrationId:guid}/submissions", SubmitRegistration).RequireAuthorization("ExamRegistrations.Submit");
        group.MapGet("/registrations/{registrationId:guid}/submissions", GetRegistrationSubmissions).RequireAuthorization("ExamRegistrations.Read");
        group.MapPost("/registrations/{registrationId:guid}/submissions/{submissionId:guid}/retry", RetryRegistrationSubmission).RequireAuthorization("ExamRegistrations.Submit");
        group.MapPost("/registrations/{registrationId:guid}/submissions/{submissionId:guid}/official-response", RecordOfficialResponse).RequireAuthorization("ExamRegistrations.ResolveErrors");
        return app;
    }

    private static async Task<IResult> HoldPlace(Guid placeId, HoldExamPlaceRequest request, IMediator mediator,
        ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actorUserId) return Results.Unauthorized();
        Result<ExamPlaceHoldResponse> result = await mediator.Send(new HoldExamPlaceCommand(
            organizationId, new ExamPlaceId(placeId), request.HoldMinutes ?? 5, actorUserId), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> ReleasePlace(Guid placeId, ReleaseExamPlaceHoldRequest request, IMediator mediator,
        ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actorUserId) return Results.Unauthorized();
        Result result = await mediator.Send(new ReleaseExamPlaceHoldCommand(
            organizationId, new ExamPlaceId(placeId), request.HoldToken, actorUserId), cancellationToken);
        return result.IsSuccess ? Results.NoContent() : ToProblem(result.Error);
    }

    private static async Task<IResult> CreateRegistration(CreateExamRegistrationRequest request, IMediator mediator,
        ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actorUserId) return Results.Unauthorized();
        Result<ExamRegistrationResponse> result = await mediator.Send(new CreateExamRegistrationCommand(
            organizationId, new PersonId(request.StudentId), new TrainingPathId(request.TrainingPathId),
            new ExamPlaceId(request.ExamPlaceId), request.HoldToken, request.OperationId, actorUserId), cancellationToken);
        return result.IsSuccess
            ? Results.Created($"/api/exams/registrations/{result.Value.Id}", result.Value)
            : ToProblem(result.Error);
    }

    private static async Task<IResult> GetRegistration(Guid registrationId, IMediator mediator, ICurrentTenant tenant,
        CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId) return Results.Unauthorized();
        Result<ExamRegistrationResponse> result = await mediator.Send(new GetExamRegistrationQuery(
            organizationId, new ExamRegistrationId(registrationId)), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> GetStudentRegistrations(Guid studentId, IMediator mediator, ICurrentTenant tenant,
        CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId) return Results.Unauthorized();
        Result<IReadOnlyList<ExamRegistrationResponse>> result = await mediator.Send(new GetStudentExamRegistrationsQuery(
            organizationId, new PersonId(studentId)), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }


    private static async Task<IResult> GetRegistrationFile(Guid registrationId, IMediator mediator, ICurrentTenant tenant, CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId) return Results.Unauthorized();
        Result<ExamRegistrationFileResponse> result = await mediator.Send(new GetExamRegistrationFileQuery(
            organizationId, new ExamRegistrationId(registrationId)), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }


    private static async Task<IResult> ExportRegistrationFile(Guid registrationId, IMediator mediator, ICurrentTenant tenant, CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId) return Results.Unauthorized();
        Result<ExamRegistrationFileResponse> result = await mediator.Send(new GetExamRegistrationFileQuery(
            organizationId, new ExamRegistrationId(registrationId)), cancellationToken);
        if (result.IsFailure) return ToProblem(result.Error);

        byte[] payload = JsonSerializer.SerializeToUtf8Bytes(result.Value, new JsonSerializerOptions(JsonSerializerDefaults.Web) { WriteIndented = true });
        return Results.File(payload, "application/json", $"exam-registration-{registrationId:N}-file-v{result.Value.CurrentVersion}.json");
    }

    private static async Task<IResult> RefreshRegistrationFile(Guid registrationId, IMediator mediator, ICurrentTenant tenant,
        ICurrentUser currentUser, CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actorUserId) return Results.Unauthorized();
        Result<ExamRegistrationFileResponse> result = await mediator.Send(new RefreshExamRegistrationFileCommand(
            organizationId, new ExamRegistrationId(registrationId), actorUserId), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> UpdateOfficialData(Guid registrationId, UpdateExamRegistrationOfficialDataRequest request,
        IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actorUserId) return Results.Unauthorized();
        Result<ExamRegistrationFileResponse> result = await mediator.Send(new UpdateExamRegistrationOfficialDataCommand(
            organizationId, new ExamRegistrationId(registrationId), request.CandidateReference, actorUserId), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> SubmitRegistration(Guid registrationId, SubmitExamRegistrationRequest request,
        IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actorUserId) return Results.Unauthorized();
        Result<ExamRegistrationSubmissionResponse> result = await mediator.Send(new SubmitExamRegistrationCommand(
            organizationId, new ExamRegistrationId(registrationId), request.OperationId, actorUserId), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> GetRegistrationSubmissions(Guid registrationId, IMediator mediator,
        ICurrentTenant tenant, CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId) return Results.Unauthorized();
        Result<IReadOnlyList<ExamRegistrationSubmissionResponse>> result = await mediator.Send(
            new GetExamRegistrationSubmissionsQuery(organizationId, new ExamRegistrationId(registrationId)), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> RetryRegistrationSubmission(Guid registrationId, Guid submissionId,
        IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actorUserId) return Results.Unauthorized();
        Result<ExamRegistrationSubmissionResponse> result = await mediator.Send(new RetryExamRegistrationSubmissionCommand(
            organizationId, new ExamRegistrationId(registrationId), new ExamRegistrationSubmissionId(submissionId), actorUserId), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> RecordOfficialResponse(Guid registrationId, Guid submissionId,
        RecordExamRegistrationOfficialResponseRequest request, IMediator mediator, ICurrentTenant tenant,
        ICurrentUser currentUser, CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actorUserId) return Results.Unauthorized();
        Result<ExamRegistrationSubmissionResponse> result = await mediator.Send(new RecordExamRegistrationOfficialResponseCommand(
            organizationId,
            new ExamRegistrationId(registrationId),
            new ExamRegistrationSubmissionId(submissionId),
            request.Outcome,
            request.ExternalSubmissionId,
            request.ExternalRegistrationId,
            request.CandidateReference,
            request.ProviderResponseCode,
            request.ProviderResponseJson,
            request.ProviderErrorCode,
            actorUserId), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static IResult ToProblem(Error error) => Results.Problem(statusCode: error.Type switch
    {
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        _ => StatusCodes.Status400BadRequest
    }, title: error.Code, detail: error.MessageKey);
}

public sealed record HoldExamPlaceRequest(int? HoldMinutes);
public sealed record ReleaseExamPlaceHoldRequest(Guid HoldToken);
public sealed record CreateExamRegistrationRequest(Guid StudentId, Guid TrainingPathId, Guid ExamPlaceId, Guid HoldToken, Guid OperationId);

public sealed record UpdateExamRegistrationOfficialDataRequest(string CandidateReference);
public sealed record SubmitExamRegistrationRequest(Guid OperationId);
public sealed record RecordExamRegistrationOfficialResponseRequest(
    OfficialExamRegistrationOutcome Outcome,
    string? ExternalSubmissionId,
    string? ExternalRegistrationId,
    string? CandidateReference,
    string? ProviderResponseCode,
    string? ProviderResponseJson,
    string? ProviderErrorCode);
