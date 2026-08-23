using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.Workforce.Application.Dashboard;

namespace DriveOS.Api.Endpoints.Workforce;

internal static class WorkforceDashboardEndpoints
{
    internal static IEndpointRouteBuilder MapWorkforceDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/workforce/dashboard")
            .WithTags("Workforce - Dashboard");

        group.MapGet("/", GetDashboard)
            .RequireAuthorization("Workforce.Dashboard.Read");

        return app;
    }

    private static async Task<IResult> GetDashboard(
        int alertWindowDays,
        IMediator mediator,
        ICurrentTenant tenant,
        CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId)
            return Results.Unauthorized();

        var normalizedWindow = alertWindowDays <= 0 ? 30 : alertWindowDays;
        var result = await mediator.Send(
            new GetWorkforceDashboardQuery(organizationId, normalizedWindow),
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
