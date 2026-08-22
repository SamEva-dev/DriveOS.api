using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.ExamsCertification.Application.Registrations.Operations;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.ExamsCertification;

internal static class ExamOperationalPlanningEndpoints
{
    internal static IEndpointRouteBuilder MapExamOperationalPlanningEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/exams/registrations/{registrationId:guid}/operational-plan").WithTags("Exams - Operational planning");
        group.MapGet("", Get).RequireAuthorization("ExamRegistrations.Read");
        group.MapGet("/options", Options).RequireAuthorization("ExamRegistrations.Read");
        group.MapPut("", Refresh).RequireAuthorization("ExamRegistrations.Update");
        return app;
    }

    private static async Task<IResult> Get(Guid registrationId, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } o) return Results.Unauthorized();
        Result<ExamOperationalPlanResponse> r = await mediator.Send(new GetExamOperationalPlanQuery(o, new ExamRegistrationId(registrationId)), ct);
        return r.IsSuccess ? Results.Ok(r.Value) : Problem(r.Error);
    }

    private static async Task<IResult> Options(Guid registrationId, Guid? departureBranchId, DateTimeOffset? meetingAtUtc, int? beforeMinutes, int? afterMinutes, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } o) return Results.Unauthorized();
        Result<ExamOperationalPlanningOptionsResponse> r = await mediator.Send(new GetExamOperationalPlanningOptionsQuery(o, new ExamRegistrationId(registrationId),
            departureBranchId.HasValue ? new BranchId(departureBranchId.Value) : null, meetingAtUtc, beforeMinutes ?? 15, afterMinutes ?? 30), ct);
        return r.IsSuccess ? Results.Ok(r.Value) : Problem(r.Error);
    }

    private static async Task<IResult> Refresh(Guid registrationId, RefreshExamOperationalPlanRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } o || user.UserId is not { } actor) return Results.Unauthorized();
        Result<ExamOperationalPlanResponse> r = await mediator.Send(new RefreshExamOperationalPlanCommand(o, new ExamRegistrationId(registrationId), request.MeetingAtUtc,
            request.TravelBufferBeforeMinutes, request.TravelBufferAfterMinutes, request.DepartureBranchId.HasValue ? new BranchId(request.DepartureBranchId.Value) : null,
            request.InstructorRequired, request.VehicleRequired, request.MeetingInstructions, actor), ct);
        return r.IsSuccess ? Results.Ok(r.Value) : Problem(r.Error);
    }

    private static IResult Problem(Error e) => Results.Problem(statusCode: e.Type switch { ErrorType.NotFound => 404, ErrorType.Conflict => 409, ErrorType.Forbidden => 403, _ => 400 }, title: e.Code, detail: e.MessageKey);
}

public sealed record RefreshExamOperationalPlanRequest(DateTimeOffset? MeetingAtUtc, int TravelBufferBeforeMinutes, int TravelBufferAfterMinutes, Guid? DepartureBranchId, bool InstructorRequired = true, bool VehicleRequired = true, string? MeetingInstructions = null);
