using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.CurriculumPedagogy.Application.PedagogicalReviews;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.CurriculumPedagogy;

internal static class PedagogicalReviewEndpoints
{
    internal static RouteGroupBuilder MapPedagogicalReviewEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/training-paths/{trainingPathId:guid}/reviews", List).RequireAuthorization("Pedagogy.Reviews.Read");
        group.MapGet("/pedagogical-reviews/{reviewId:guid}", Get).RequireAuthorization("Pedagogy.Reviews.Read");
        group.MapPost("/training-paths/{trainingPathId:guid}/reviews", Request).RequireAuthorization("Pedagogy.ReviewRequest");
        group.MapPost("/pedagogical-reviews/{reviewId:guid}/start", Start).RequireAuthorization("Pedagogy.Reviews.Manage");
        group.MapPost("/pedagogical-reviews/{reviewId:guid}/complete", Complete).RequireAuthorization("Pedagogy.Reviews.Manage");
        group.MapPost("/pedagogical-reviews/{reviewId:guid}/cancel", Cancel).RequireAuthorization("Pedagogy.Reviews.Manage");
        return group;
    }

    private static IResult Fail(Error error) => Results.Problem(statusCode: error.Type == ErrorType.NotFound ? 404 : error.Type == ErrorType.Conflict ? 409 : 400, title: error.Code, detail: error.MessageKey);
    private static bool Context(ICurrentTenant tenant, ICurrentUser user, out OrganizationId organizationId, out UserId actor) { organizationId = tenant.OrganizationId ?? OrganizationId.Empty; actor = user.UserId ?? UserId.Empty; return !organizationId.IsEmpty && !actor.IsEmpty; }

    private static async Task<IResult> List(Guid trainingPathId, IPedagogicalReviewReadService reads, ICurrentTenant tenant, CancellationToken ct)
    { if (tenant.OrganizationId is not { } org) return Results.Unauthorized(); return Results.Ok(await reads.ListForTrainingPathAsync(org, new TrainingPathId(trainingPathId), ct)); }
    private static async Task<IResult> Get(Guid reviewId, IPedagogicalReviewReadService reads, ICurrentTenant tenant, CancellationToken ct)
    { if (tenant.OrganizationId is not { } org) return Results.Unauthorized(); var r = await reads.GetAsync(org, new PedagogicalReviewId(reviewId), ct); return r is null ? Results.NotFound() : Results.Ok(r); }
    private static async Task<IResult> Request(Guid trainingPathId, RequestPedagogicalReviewRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    { if (!Context(tenant, user, out var org, out var actor)) return Results.Unauthorized(); var r = await mediator.Send(new RequestPedagogicalReviewCommand(org, new TrainingPathId(trainingPathId), actor, request.Reason, actor), ct); return r.IsSuccess ? Results.Created($"/api/pedagogy/pedagogical-reviews/{r.Value.Value}", new { id = r.Value.Value }) : Fail(r.Error); }
    private static async Task<IResult> Start(Guid reviewId, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    { if (!Context(tenant, user, out var org, out var actor)) return Results.Unauthorized(); var r = await mediator.Send(new StartPedagogicalReviewCommand(org, new PedagogicalReviewId(reviewId), actor), ct); return r.IsSuccess ? Results.NoContent() : Fail(r.Error); }
    private static async Task<IResult> Complete(Guid reviewId, CompletePedagogicalReviewRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    { if (!Context(tenant, user, out var org, out var actor)) return Results.Unauthorized(); var r = await mediator.Send(new CompletePedagogicalReviewCommand(org, new PedagogicalReviewId(reviewId), request.Findings, request.Recommendations, request.EstimatedRemainingPracticalHours, actor), ct); return r.IsSuccess ? Results.NoContent() : Fail(r.Error); }
    private static async Task<IResult> Cancel(Guid reviewId, CancelPedagogicalReviewRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    { if (!Context(tenant, user, out var org, out var actor)) return Results.Unauthorized(); var r = await mediator.Send(new CancelPedagogicalReviewCommand(org, new PedagogicalReviewId(reviewId), request.Reason, actor), ct); return r.IsSuccess ? Results.NoContent() : Fail(r.Error); }
}

public sealed record RequestPedagogicalReviewRequest(string Reason);
public sealed record CompletePedagogicalReviewRequest(string Findings, string Recommendations, decimal? EstimatedRemainingPracticalHours);
public sealed record CancelPedagogicalReviewRequest(string Reason);
