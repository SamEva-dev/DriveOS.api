using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.ExamsCertification.Application.Registrations.Convocations;
using DriveOS.Modules.ExamsCertification.Domain.Registrations.Convocations;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.ExamsCertification;

internal static class ExamConvocationEndpoints
{
    internal static IEndpointRouteBuilder MapExamConvocationEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/exams/registrations/{registrationId:guid}/convocation")
            .WithTags("Exams - Convocations");

        group.MapGet("", Get).RequireAuthorization("ExamRegistrations.Read");
        group.MapPost("", Receive).RequireAuthorization("ExamRegistrations.Update");
        group.MapPut("/meeting", SetMeeting).RequireAuthorization("ExamRegistrations.Update");
        group.MapPost("/delivered", MarkDelivered).RequireAuthorization("ExamRegistrations.Update");
        group.MapPost("/acknowledged", MarkAcknowledged).RequireAuthorization("ExamRegistrations.Update");
        return app;
    }

    private static async Task<IResult> Get(Guid registrationId, IMediator mediator, ICurrentTenant tenant, CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId) return Results.Unauthorized();
        Result<ExamConvocationResponse> result = await mediator.Send(
            new GetExamConvocationQuery(organizationId, new ExamRegistrationId(registrationId)), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> Receive(Guid registrationId, ReceiveExamConvocationRequest request,
        IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actorUserId)
            return Results.Unauthorized();

        Result<ExamConvocationResponse> result = await mediator.Send(new ReceiveExamConvocationCommand(
            organizationId,
            new ExamRegistrationId(registrationId),
            new ExamCenterId(request.ExamCenterId),
            request.ScheduledStartUtc,
            request.ScheduledEndUtc,
            request.ProviderCode,
            request.OfficialReference,
            request.CandidateReference,
            request.Instructions,
            request.RequiredDocuments,
            request.ProviderPayloadReference,
            request.OperationId,
            actorUserId), cancellationToken);

        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> SetMeeting(Guid registrationId, SetExamConvocationMeetingRequest request,
        IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actorUserId)
            return Results.Unauthorized();

        Result<ExamConvocationResponse> result = await mediator.Send(new SetExamConvocationMeetingCommand(
            organizationId, new ExamRegistrationId(registrationId), request.MeetingAtUtc, request.Instructions, actorUserId), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> MarkDelivered(Guid registrationId, MarkExamConvocationDeliveredRequest request,
        IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actorUserId)
            return Results.Unauthorized();

        Result<ExamConvocationResponse> result = await mediator.Send(new MarkExamConvocationDeliveredCommand(
            organizationId, new ExamRegistrationId(registrationId), request.Channel, actorUserId), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> MarkAcknowledged(Guid registrationId, IMediator mediator,
        ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actorUserId)
            return Results.Unauthorized();

        Result<ExamConvocationResponse> result = await mediator.Send(new MarkExamConvocationAcknowledgedCommand(
            organizationId, new ExamRegistrationId(registrationId), actorUserId), cancellationToken);
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

public sealed record ReceiveExamConvocationRequest(
    Guid ExamCenterId,
    DateTimeOffset ScheduledStartUtc,
    DateTimeOffset ScheduledEndUtc,
    string ProviderCode,
    string? OfficialReference,
    string? CandidateReference,
    string? Instructions,
    string? RequiredDocuments,
    string? ProviderPayloadReference,
    Guid OperationId);

public sealed record SetExamConvocationMeetingRequest(DateTimeOffset? MeetingAtUtc, string? Instructions);
public sealed record MarkExamConvocationDeliveredRequest(ExamConvocationDeliveryChannel Channel);
