using DriveOS.Modules.Organizations.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Api.Endpoints.Operations;

internal static class HealthEndpoints
{
    internal static IEndpointRouteBuilder MapDriveOsHealthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        static IResult Live() => Results.Ok(new
        {
            status = "Healthy",
            service = "DriveOS.Api",
            checkedAtUtc = DateTimeOffset.UtcNow
        });
        endpoints.MapGet("/health", Live).AllowAnonymous();
        endpoints.MapGet("/health/live", Live).AllowAnonymous();

        endpoints.MapGet("/health/ready", ReadinessAsync).AllowAnonymous();
        return endpoints;
    }

    private static async Task<IResult> ReadinessAsync(
        OrganizationsDbContext database,
        CancellationToken cancellationToken)
    {
        try
        {
            bool available = await database.Database.CanConnectAsync(cancellationToken);
            return available
                ? Results.Ok(new
                {
                    status = "Healthy",
                    service = "DriveOS.Api",
                    dependencies = new { database = "Healthy" },
                    checkedAtUtc = DateTimeOffset.UtcNow
                })
                : Results.Json(new
                {
                    status = "Unhealthy",
                    service = "DriveOS.Api",
                    dependencies = new { database = "Unhealthy" },
                    checkedAtUtc = DateTimeOffset.UtcNow
                }, statusCode: StatusCodes.Status503ServiceUnavailable);
        }
        catch
        {
            return Results.Json(new
            {
                status = "Unhealthy",
                service = "DriveOS.Api",
                dependencies = new { database = "Unhealthy" },
                checkedAtUtc = DateTimeOffset.UtcNow
            }, statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }
}
