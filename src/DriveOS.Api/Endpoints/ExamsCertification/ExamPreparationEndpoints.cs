using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.ExamsCertification.Application.Registrations.Preparation;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using DriveOS.Security.Contracts;

namespace DriveOS.Api.Endpoints.ExamsCertification;

internal static class ExamPreparationEndpoints
{
    internal static IEndpointRouteBuilder MapExamPreparationEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapGroup("/api/exams/registrations/{registrationId:guid}/preparation")
            .WithTags("Exams & Certification");

        group.MapGet("/", Get)
            .RequireAuthorization(DriveOsPermissionCodes.Exams.PreparationRead);

        group.MapPut("/", Refresh)
            .RequireAuthorization(DriveOsPermissionCodes.Exams.PreparationManage);

        group.MapPost("/confirm", Confirm)
            .RequireAuthorization(DriveOsPermissionCodes.Exams.PreparationManage);

        return app;
    }

    private static async Task<IResult> Get(
        Guid registrationId,
        IMediator mediator,
        ICurrentTenant tenant,
        CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId)
            return Results.Unauthorized();

        Result<ExamPreparationResponse> result = await mediator.Send(
            new GetExamPreparationQuery(organizationId, new ExamRegistrationId(registrationId)),
            cancellationToken);

        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private static async Task<IResult> Refresh(
        Guid registrationId,
        RefreshExamPreparationRequest request,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actor)
            return Results.Unauthorized();

        Result<ExamPreparationResponse> result = await mediator.Send(
            new RefreshExamPreparationCommand(
                organizationId,
                new ExamRegistrationId(registrationId),
                request.MeetingPointConfirmed,
                request.VehicleEnergyConfirmed,
                request.InstructorConfirmed,
                request.InstructionsTransmitted,
                request.ReminderOffsetsDays,
                request.OperationId,
                actor),
            cancellationToken);

        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private static async Task<IResult> Confirm(
        Guid registrationId,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actor)
            return Results.Unauthorized();

        Result<ExamPreparationResponse> result = await mediator.Send(
            new ConfirmExamPreparationCommand(
                organizationId,
                new ExamRegistrationId(registrationId),
                actor),
            cancellationToken);

        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private static IResult Failure(Error error) => Results.Problem(
        statusCode: error.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status400BadRequest
        },
        title: error.Code,
        extensions: new Dictionary<string, object?>
        {
            ["code"] = error.Code,
            ["messageKey"] = error.MessageKey
        });
}

internal sealed record RefreshExamPreparationRequest(
    bool MeetingPointConfirmed,
    bool VehicleEnergyConfirmed,
    bool InstructorConfirmed,
    bool InstructionsTransmitted,
    IReadOnlyCollection<int>? ReminderOffsetsDays,
    Guid OperationId);
