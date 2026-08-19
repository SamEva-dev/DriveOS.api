using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.CurriculumPedagogy.Application.Progression;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Api.Endpoints.CurriculumPedagogy;

internal static class PedagogicalProgressionEndpoints
{
    internal static RouteGroupBuilder MapPedagogicalProgressionEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/training-paths/{trainingPathId:guid}/progression", GetOverview)
            .RequireAuthorization("Pedagogy.Summary.Read");
        group.MapGet("/training-paths/{trainingPathId:guid}/progression/history", GetHistory)
            .RequireAuthorization("Pedagogy.Summary.Read");
        return group;
    }

    private static async Task<IResult> GetOverview(
        Guid trainingPathId,
        int? recent,
        IPedagogicalProgressionReadService readService,
        ICurrentTenant currentTenant,
        CancellationToken cancellationToken)
    {
        if (currentTenant.OrganizationId is not { } organizationId)
            return Results.Unauthorized();

        PedagogicalProgressionOverviewResponse? response = await readService.GetOverviewAsync(
            organizationId,
            new TrainingPathId(trainingPathId),
            includeInternalComments: true,
            recentTimelineLimit: Math.Clamp(recent ?? 50, 1, 200),
            cancellationToken: cancellationToken);

        return response is null ? Results.NotFound() : Results.Ok(response);
    }

    private static async Task<IResult> GetHistory(
        Guid trainingPathId,
        IPedagogicalProgressionReadService readService,
        ICurrentTenant currentTenant,
        CancellationToken cancellationToken)
    {
        if (currentTenant.OrganizationId is not { } organizationId)
            return Results.Unauthorized();

        IReadOnlyCollection<PedagogicalProgressionTimelineItemResponse>? response =
            await readService.GetHistoryAsync(
                organizationId,
                new TrainingPathId(trainingPathId),
                includeInternalComments: true,
                cancellationToken: cancellationToken);

        return response is null ? Results.NotFound() : Results.Ok(response);
    }
}
