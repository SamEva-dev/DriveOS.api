using System.Net.Http.Headers;

namespace DriveOS.Api.Security;

public sealed class AccessManagerMachineTokenEndpointFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        if (!AuthenticationHeaderValue.TryParse(httpContext.Request.Headers.Authorization, out var header)
            || !string.Equals(header.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(header.Parameter))
            return Results.Unauthorized();

        var validator = httpContext.RequestServices.GetRequiredService<IAuthGateMachineTokenValidator>();
        var principal = await validator.ValidateAsync(
            header.Parameter,
            httpContext.RequestAborted,
            requiredClientId: "manager-api",
            requiredScope: "driveos.access-management");

        if (principal is null) return Results.Forbid();
        httpContext.User = principal;
        return await next(context);
    }
}
