using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.CurriculumPedagogy.Application.TrainingPaths;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.CurriculumPedagogy;

internal static class TrainingPathEndpoints
{
    internal static RouteGroupBuilder MapTrainingPathEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/students/{studentId:guid}/training-paths", ListForStudent)
            .RequireAuthorization("Pedagogy.TrainingPaths.Read");
        group.MapGet("/training-paths/{trainingPathId:guid}", Get)
            .RequireAuthorization("Pedagogy.TrainingPaths.Read");
        group.MapPost("/students/{studentId:guid}/training-paths", Create)
            .RequireAuthorization("Pedagogy.TrainingPaths.Create");

        group.MapPost("/training-paths/{trainingPathId:guid}/mark-ready", MarkReady)
            .RequireAuthorization("Pedagogy.TrainingPaths.Manage");
        group.MapPost("/training-paths/{trainingPathId:guid}/activate", Activate)
            .RequireAuthorization("Pedagogy.TrainingPaths.Activate");
        group.MapPost("/training-paths/{trainingPathId:guid}/suspend", Suspend)
            .RequireAuthorization("Pedagogy.TrainingPaths.Manage");
        group.MapPost("/training-paths/{trainingPathId:guid}/reactivate", Reactivate)
            .RequireAuthorization("Pedagogy.TrainingPaths.Manage");
        group.MapPost("/training-paths/{trainingPathId:guid}/complete", Complete)
            .RequireAuthorization("Pedagogy.TrainingPaths.Manage");
        group.MapPost("/training-paths/{trainingPathId:guid}/cancel", Cancel)
            .RequireAuthorization("Pedagogy.TrainingPaths.Manage");

        group.MapPost("/training-paths/{trainingPathId:guid}/milestones", AddMilestone)
            .RequireAuthorization("Pedagogy.TrainingPaths.Manage");
        group.MapPost("/training-paths/{trainingPathId:guid}/milestones/{milestoneId:guid}/start", StartMilestone)
            .RequireAuthorization("Pedagogy.TrainingPaths.Manage");
        group.MapPost("/training-paths/{trainingPathId:guid}/milestones/{milestoneId:guid}/complete", CompleteMilestone)
            .RequireAuthorization("Pedagogy.TrainingPaths.Manage");
        group.MapPost("/training-paths/{trainingPathId:guid}/milestones/{milestoneId:guid}/cancel", CancelMilestone)
            .RequireAuthorization("Pedagogy.TrainingPaths.Manage");
        return group;
    }

    private static IResult Fail(Error error) => Results.Problem(
        statusCode: error.Type == ErrorType.NotFound ? StatusCodes.Status404NotFound :
                    error.Type == ErrorType.Conflict ? StatusCodes.Status409Conflict :
                    StatusCodes.Status400BadRequest,
        title: error.Code,
        detail: error.MessageKey);

    private static bool TryContext(ICurrentTenant currentTenant, ICurrentUser currentUser, out OrganizationId organizationId, out UserId actorUserId)
    {
        organizationId = default;
        actorUserId = default;
        if (currentTenant.OrganizationId is not { } organization || currentUser.UserId is not { } actor)
            return false;
        organizationId = organization;
        actorUserId = actor;
        return true;
    }

    private static async Task<IResult> ListForStudent(
        Guid studentId,
        ITrainingPathReadService readService,
        ICurrentTenant currentTenant,
        CancellationToken cancellationToken)
    {
        if (currentTenant.OrganizationId is not { } organizationId) return Results.Unauthorized();
        return Results.Ok(await readService.ListForStudentAsync(
            organizationId,
            new PersonId(studentId),
            cancellationToken));
    }

    private static async Task<IResult> Get(
        Guid trainingPathId,
        ITrainingPathReadService readService,
        ICurrentTenant currentTenant,
        CancellationToken cancellationToken)
    {
        if (currentTenant.OrganizationId is not { } organizationId) return Results.Unauthorized();
        TrainingPathDetailResponse? response = await readService.GetAsync(
            organizationId,
            new TrainingPathId(trainingPathId),
            cancellationToken);
        return response is null ? Results.NotFound() : Results.Ok(response);
    }

    private static async Task<IResult> Create(
        Guid studentId,
        CreateTrainingPathRequest request,
        IMediator mediator,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        if (!TryContext(currentTenant, currentUser, out OrganizationId organizationId, out UserId actorUserId))
            return Results.Unauthorized();

        Result<TrainingPathId> result = await mediator.Send(new CreateTrainingPathCommand(
            organizationId,
            new PersonId(studentId),
            new CurriculumVersionId(request.CurriculumVersionId),
            request.TrainingMode,
            request.StartDate,
            request.TargetCompletionDate,
            request.EstimatedPracticalHours,
            actorUserId), cancellationToken);

        return result.IsSuccess
            ? Results.Created($"/api/pedagogy/training-paths/{result.Value.Value}", new { id = result.Value.Value })
            : Fail(result.Error);
    }

    private static async Task<IResult> MarkReady(Guid trainingPathId, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!TryContext(tenant, user, out var organizationId, out var actor)) return Results.Unauthorized();
        Result result = await mediator.Send(new MarkTrainingPathReadyCommand(organizationId, new(trainingPathId), actor), ct);
        return result.IsSuccess ? Results.NoContent() : Fail(result.Error);
    }

    private static async Task<IResult> Activate(Guid trainingPathId, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!TryContext(tenant, user, out var organizationId, out var actor)) return Results.Unauthorized();
        Result result = await mediator.Send(new ActivateTrainingPathCommand(organizationId, new(trainingPathId), actor), ct);
        return result.IsSuccess ? Results.NoContent() : Fail(result.Error);
    }

    private static async Task<IResult> Suspend(Guid trainingPathId, TrainingPathReasonRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!TryContext(tenant, user, out var organizationId, out var actor)) return Results.Unauthorized();
        Result result = await mediator.Send(new SuspendTrainingPathCommand(organizationId, new(trainingPathId), request.Reason, actor), ct);
        return result.IsSuccess ? Results.NoContent() : Fail(result.Error);
    }

    private static async Task<IResult> Reactivate(Guid trainingPathId, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!TryContext(tenant, user, out var organizationId, out var actor)) return Results.Unauthorized();
        Result result = await mediator.Send(new ReactivateTrainingPathCommand(organizationId, new(trainingPathId), actor), ct);
        return result.IsSuccess ? Results.NoContent() : Fail(result.Error);
    }

    private static async Task<IResult> Complete(Guid trainingPathId, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!TryContext(tenant, user, out var organizationId, out var actor)) return Results.Unauthorized();
        Result result = await mediator.Send(new CompleteTrainingPathCommand(organizationId, new(trainingPathId), actor), ct);
        return result.IsSuccess ? Results.NoContent() : Fail(result.Error);
    }

    private static async Task<IResult> Cancel(Guid trainingPathId, TrainingPathReasonRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!TryContext(tenant, user, out var organizationId, out var actor)) return Results.Unauthorized();
        Result result = await mediator.Send(new CancelTrainingPathCommand(organizationId, new(trainingPathId), request.Reason, actor), ct);
        return result.IsSuccess ? Results.NoContent() : Fail(result.Error);
    }

    private static async Task<IResult> AddMilestone(Guid trainingPathId, AddTrainingPathMilestoneRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!TryContext(tenant, user, out var organizationId, out var actor)) return Results.Unauthorized();
        Result<TrainingPathMilestoneId> result = await mediator.Send(new AddTrainingPathMilestoneCommand(
            organizationId, new(trainingPathId), request.Code, request.Name, request.Description, request.Order, request.TargetDate, actor), ct);
        return result.IsSuccess
            ? Results.Created($"/api/pedagogy/training-paths/{trainingPathId}/milestones/{result.Value.Value}", new { id = result.Value.Value })
            : Fail(result.Error);
    }

    private static async Task<IResult> StartMilestone(Guid trainingPathId, Guid milestoneId, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!TryContext(tenant, user, out var organizationId, out var actor)) return Results.Unauthorized();
        Result result = await mediator.Send(new StartTrainingPathMilestoneCommand(organizationId, new(trainingPathId), new(milestoneId), actor), ct);
        return result.IsSuccess ? Results.NoContent() : Fail(result.Error);
    }

    private static async Task<IResult> CompleteMilestone(Guid trainingPathId, Guid milestoneId, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!TryContext(tenant, user, out var organizationId, out var actor)) return Results.Unauthorized();
        Result result = await mediator.Send(new CompleteTrainingPathMilestoneCommand(organizationId, new(trainingPathId), new(milestoneId), actor), ct);
        return result.IsSuccess ? Results.NoContent() : Fail(result.Error);
    }

    private static async Task<IResult> CancelMilestone(Guid trainingPathId, Guid milestoneId, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!TryContext(tenant, user, out var organizationId, out var actor)) return Results.Unauthorized();
        Result result = await mediator.Send(new CancelTrainingPathMilestoneCommand(organizationId, new(trainingPathId), new(milestoneId), actor), ct);
        return result.IsSuccess ? Results.NoContent() : Fail(result.Error);
    }
}

public sealed record CreateTrainingPathRequest(
    Guid CurriculumVersionId,
    int TrainingMode,
    DateOnly StartDate,
    DateOnly? TargetCompletionDate,
    decimal? EstimatedPracticalHours);

public sealed record TrainingPathReasonRequest(string Reason);

public sealed record AddTrainingPathMilestoneRequest(
    string Code,
    string Name,
    string? Description,
    int Order,
    DateOnly? TargetDate);
