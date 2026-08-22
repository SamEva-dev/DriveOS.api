using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.ExamsCertification.Application.Registrations.Assignments;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.ExamsCertification;

internal static class ExamResourceAssignmentEndpoints
{
    internal static IEndpointRouteBuilder MapExamResourceAssignmentEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/exams/registrations/{registrationId:guid}/resources").WithTags("Exams & Certification");
        group.MapGet("/", Get).RequireAuthorization("ExamRegistrations.Read");
        group.MapPost("/assign", Assign).RequireAuthorization("ExamRegistrations.Update");
        return app;
    }

    private static async Task<IResult> Get(Guid registrationId, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId) return Results.Unauthorized();
        Result<ExamResourceAssignmentResponse> result = await mediator.Send(new GetExamResourceAssignmentQuery(organizationId, new ExamRegistrationId(registrationId)), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private static async Task<IResult> Assign(Guid registrationId, AssignExamResourcesRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actor) return Results.Unauthorized();
        Result<ExamResourceAssignmentResponse> result = await mediator.Send(new AssignExamResourcesCommand(
            organizationId, new ExamRegistrationId(registrationId),
            request.InstructorCalendarResourceId.HasValue ? new CalendarResourceId(request.InstructorCalendarResourceId.Value) : null,
            request.VehicleCalendarResourceId.HasValue ? new CalendarResourceId(request.VehicleCalendarResourceId.Value) : null,
            request.TrainingCategory, request.TransmissionType, request.DualControlRequired, request.RequiredAdaptations ?? [], request.EnergyType,
            request.OperationId, actor), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private static IResult Failure(Error error) => Results.Problem(statusCode: error.Type switch
    {
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        _ => StatusCodes.Status400BadRequest
    }, title: error.Code, extensions: new Dictionary<string, object?> { ["code"] = error.Code, ["messageKey"] = error.MessageKey });
}

internal sealed record AssignExamResourcesRequest(
    Guid? InstructorCalendarResourceId,
    Guid? VehicleCalendarResourceId,
    string TrainingCategory,
    string? TransmissionType,
    bool DualControlRequired,
    IReadOnlyCollection<string>? RequiredAdaptations,
    string? EnergyType,
    Guid OperationId);
