using System.Net.Http.Headers;

namespace DriveOS.Api.Security;

public sealed class AuthGateMachineTokenEndpointFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next
    )
    {
        HttpContext httpContext = context.HttpContext;
        string authorization = httpContext.Request.Headers.Authorization.ToString();

        if (
            !AuthenticationHeaderValue.TryParse(
                authorization,
                out AuthenticationHeaderValue? header
            )
            || !string.Equals(header.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(header.Parameter)
        )
        {
            return Results.Unauthorized();
        }

        IAuthGateMachineTokenValidator validator =
            httpContext.RequestServices.GetRequiredService<IAuthGateMachineTokenValidator>();

        var principal = await validator.ValidateAsync(header.Parameter, httpContext.RequestAborted);

        if (principal is null)
        {
            return Results.Forbid();
        }

        httpContext.User = principal;
        return await next(context);
    }
}
