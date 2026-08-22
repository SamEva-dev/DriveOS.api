using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.ExamsCertification.Application.Registrations.Attempts;
using DriveOS.Security.Contracts;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.ExamsCertification;

internal static class ExamAttemptEndpoints
{
    internal static IEndpointRouteBuilder MapExamAttemptEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/exams/registrations/{registrationId:guid}/attempt").WithTags("Exams & Certification");
        group.MapGet("/", Get).RequireAuthorization(DriveOsPermissionCodes.Exams.AttemptsRead);
        group.MapPost("/", Create).RequireAuthorization(DriveOsPermissionCodes.Exams.AttemptsManage);
        group.MapPost("/check-in", CheckIn).RequireAuthorization(DriveOsPermissionCodes.Exams.AttemptsManage);
        group.MapPost("/departure", Departure).RequireAuthorization(DriveOsPermissionCodes.Exams.AttemptsManage);
        group.MapPost("/arrival", Arrival).RequireAuthorization(DriveOsPermissionCodes.Exams.AttemptsManage);
        group.MapPost("/start", Start).RequireAuthorization(DriveOsPermissionCodes.Exams.AttemptsManage);
        group.MapPost("/complete", Complete).RequireAuthorization(DriveOsPermissionCodes.Exams.AttemptsManage);
        group.MapPost("/return", Return).RequireAuthorization(DriveOsPermissionCodes.Exams.AttemptsManage);
        group.MapPost("/incident", Incident).RequireAuthorization(DriveOsPermissionCodes.Exams.AttemptsManage);
        group.MapPost("/note", Note).RequireAuthorization(DriveOsPermissionCodes.Exams.AttemptsManage);
        group.MapPost("/location", Location).RequireAuthorization(DriveOsPermissionCodes.Exams.AttemptsManage);
        group.MapPost("/resource-change", ResourceChange).RequireAuthorization(DriveOsPermissionCodes.Exams.AttemptsManage);

        group.MapPost("/absent", MarkAbsent).RequireAuthorization(DriveOsPermissionCodes.Exams.AttemptsManage);
        group.MapPost("/postpone", Postpone).RequireAuthorization(DriveOsPermissionCodes.Exams.AttemptsManage);
        group.MapPost("/cancel", Cancel).RequireAuthorization(DriveOsPermissionCodes.Exams.AttemptsManage);
        group.MapPost("/interrupt", Interrupt).RequireAuthorization(DriveOsPermissionCodes.Exams.AttemptsManage);
        group.MapPost("/unable-to-start", UnableToStart).RequireAuthorization(DriveOsPermissionCodes.Exams.AttemptsManage);
        return app;
    }

    private static async Task<IResult> Get(Guid registrationId, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId) return Results.Unauthorized();
        return ToResult(await mediator.Send(new GetExamAttemptQuery(organizationId, new ExamRegistrationId(registrationId)), ct));
    }

    private static async Task<IResult> Create(Guid registrationId, OperationRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!TryContext(tenant, user, out var organizationId, out var actor)) return Results.Unauthorized();
        return ToResult(await mediator.Send(new CreateExamAttemptCommand(organizationId, new ExamRegistrationId(registrationId), request.OperationId, actor), ct));
    }

    private static async Task<IResult> CheckIn(Guid registrationId, OperationRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!TryContext(tenant, user, out var organizationId, out var actor)) return Results.Unauthorized();
        return ToResult(await mediator.Send(new CheckInExamAttemptCommand(organizationId, new ExamRegistrationId(registrationId), request.OperationId, request.OccurredAtUtc, actor), ct));
    }

    private static async Task<IResult> Start(Guid registrationId, OperationRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!TryContext(tenant, user, out var organizationId, out var actor)) return Results.Unauthorized();
        return ToResult(await mediator.Send(new StartExamAttemptCommand(organizationId, new ExamRegistrationId(registrationId), request.OperationId, request.OccurredAtUtc, actor), ct));
    }

    private static async Task<IResult> Complete(Guid registrationId, OperationRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!TryContext(tenant, user, out var organizationId, out var actor)) return Results.Unauthorized();
        return ToResult(await mediator.Send(new CompleteExamAttemptCommand(organizationId, new ExamRegistrationId(registrationId), request.OperationId, request.OccurredAtUtc, actor), ct));
    }

    private static async Task<IResult> Departure(Guid registrationId, OperationRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct) { if (!TryContext(tenant,user,out var o,out var a)) return Results.Unauthorized(); return ToResult(await mediator.Send(new RecordExamDepartureCommand(o,new ExamRegistrationId(registrationId),request.OperationId,request.OccurredAtUtc,a),ct)); }
    private static async Task<IResult> Arrival(Guid registrationId, OperationRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct) { if (!TryContext(tenant,user,out var o,out var a)) return Results.Unauthorized(); return ToResult(await mediator.Send(new RecordExamArrivalCommand(o,new ExamRegistrationId(registrationId),request.OperationId,request.OccurredAtUtc,a),ct)); }
    private static async Task<IResult> Return(Guid registrationId, OperationRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct) { if (!TryContext(tenant,user,out var o,out var a)) return Results.Unauthorized(); return ToResult(await mediator.Send(new RecordExamReturnCommand(o,new ExamRegistrationId(registrationId),request.OperationId,request.OccurredAtUtc,a),ct)); }
    private static async Task<IResult> Incident(Guid registrationId, IncidentRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct) { if (!TryContext(tenant,user,out var o,out var a)) return Results.Unauthorized(); return ToResult(await mediator.Send(new ReportExamAttemptIncidentCommand(o,new ExamRegistrationId(registrationId),request.IncidentCode,request.Description,request.OperationId,request.OccurredAtUtc,a),ct)); }
    private static async Task<IResult> Note(Guid registrationId, NoteRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct) { if (!TryContext(tenant,user,out var o,out var a)) return Results.Unauthorized(); return ToResult(await mediator.Send(new AddExamAttemptNoteCommand(o,new ExamRegistrationId(registrationId),request.Note,request.OperationId,request.OccurredAtUtc,a),ct)); }
    private static async Task<IResult> Location(Guid registrationId, LocationRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct) { if (!TryContext(tenant,user,out var o,out var a)) return Results.Unauthorized(); return ToResult(await mediator.Send(new RecordExamAttemptLocationCommand(o,new ExamRegistrationId(registrationId),request.Latitude,request.Longitude,request.AccuracyMeters,request.Purpose,request.OperationId,request.OccurredAtUtc,a),ct)); }
    private static async Task<IResult> ResourceChange(Guid registrationId, ResourceChangeRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct) { if (!TryContext(tenant,user,out var o,out var a)) return Results.Unauthorized(); return ToResult(await mediator.Send(new RecordExamAttemptResourceChangeCommand(o,new ExamRegistrationId(registrationId),request.Reason,request.OperationId,request.OccurredAtUtc,a),ct)); }

    private static async Task<IResult> MarkAbsent(Guid registrationId, AbsenceRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!TryContext(tenant, user, out var organizationId, out var actor)) return Results.Unauthorized();
        return ToResult(await mediator.Send(new MarkExamAttemptAbsentCommand(organizationId, new ExamRegistrationId(registrationId), request.Excused, request.ReasonCode, request.Notes, request.OperationId, actor), ct));
    }

    private static async Task<IResult> Postpone(Guid registrationId, ReasonRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!TryContext(tenant, user, out var organizationId, out var actor)) return Results.Unauthorized();
        return ToResult(await mediator.Send(new PostponeExamAttemptCommand(organizationId, new ExamRegistrationId(registrationId), request.ReasonCode, request.Notes, request.OperationId, actor), ct));
    }

    private static async Task<IResult> Cancel(Guid registrationId, ReasonRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!TryContext(tenant, user, out var organizationId, out var actor)) return Results.Unauthorized();
        return ToResult(await mediator.Send(new CancelExamAttemptCommand(organizationId, new ExamRegistrationId(registrationId), request.ReasonCode, request.Notes, request.OperationId, actor), ct));
    }

    private static async Task<IResult> Interrupt(Guid registrationId, ReasonRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!TryContext(tenant, user, out var organizationId, out var actor)) return Results.Unauthorized();
        return ToResult(await mediator.Send(new InterruptExamAttemptCommand(organizationId, new ExamRegistrationId(registrationId), request.ReasonCode, request.Notes, request.OperationId, actor), ct));
    }

    private static async Task<IResult> UnableToStart(Guid registrationId, ReasonRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!TryContext(tenant, user, out var organizationId, out var actor)) return Results.Unauthorized();
        return ToResult(await mediator.Send(new MarkExamAttemptUnableToStartCommand(organizationId, new ExamRegistrationId(registrationId), request.ReasonCode, request.Notes, request.OperationId, actor), ct));
    }

    private static bool TryContext(ICurrentTenant tenant, ICurrentUser user, out OrganizationId organizationId, out UserId actor)
    {
        organizationId = default;
        actor = default;
        if (tenant.OrganizationId is not { } org || user.UserId is not { } uid) return false;
        organizationId = org;
        actor = uid;
        return true;
    }

    private static IResult ToResult(Result<ExamAttemptResponse> result) => result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);

    private static IResult Failure(Error error) => Results.Problem(
        statusCode: error.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status400BadRequest
        },
        title: error.Code,
        extensions: new Dictionary<string, object?> { ["code"] = error.Code, ["messageKey"] = error.MessageKey });
}

internal sealed record OperationRequest(Guid OperationId, DateTimeOffset? OccurredAtUtc = null);
internal sealed record IncidentRequest(string IncidentCode, string Description, Guid OperationId, DateTimeOffset? OccurredAtUtc = null);
internal sealed record NoteRequest(string Note, Guid OperationId, DateTimeOffset? OccurredAtUtc = null);
internal sealed record LocationRequest(decimal Latitude, decimal Longitude, decimal? AccuracyMeters, string Purpose, Guid OperationId, DateTimeOffset? OccurredAtUtc = null);
internal sealed record ResourceChangeRequest(string Reason, Guid OperationId, DateTimeOffset? OccurredAtUtc = null);
internal sealed record ReasonRequest(string ReasonCode, string? Notes, Guid OperationId);
internal sealed record AbsenceRequest(bool Excused, string ReasonCode, string? Notes, Guid OperationId);
