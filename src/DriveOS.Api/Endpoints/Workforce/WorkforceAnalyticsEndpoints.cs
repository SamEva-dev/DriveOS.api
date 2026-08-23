using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.Workforce.Application.Analytics;

namespace DriveOS.Api.Endpoints.Workforce;

internal static class WorkforceAnalyticsEndpoints
{
    internal static IEndpointRouteBuilder MapWorkforceAnalyticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/workforce/analytics")
            .WithTags("Workforce - Analytics");

        group.MapGet("/", GetAnalytics)
            .RequireAuthorization("Workforce.Analytics.Read");

        return app;
    }

    private static async Task<IResult> GetAnalytics(
        DateOnly from,
        DateOnly to,
        IMediator mediator,
        ICurrentTenant tenant,
        CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId)
            return Results.Unauthorized();

        var result = await mediator.Send(
            new GetWorkforceAnalyticsQuery(organizationId, from, to),
            cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(
                statusCode: 400,
                extensions: new Dictionary<string, object?>
                {
                    ["code"] = result.Error.Code,
                    ["messageKey"] = result.Error.MessageKey
                });
    }
}
