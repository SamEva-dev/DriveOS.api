using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace DriveOS.Api.Infrastructure.Logging;

public sealed class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        string correlationId = ResolveCorrelationId(httpContext);

        httpContext.TraceIdentifier = correlationId;

        httpContext.Response.Headers[LoggingConstants.CorrelationIdHeader] = correlationId;

        using (LogContext.PushProperty(LoggingConstants.CorrelationIdProperty, correlationId))
        {
            await _next(httpContext);
        }
    }

    private static string ResolveCorrelationId(HttpContext httpContext)
    {
        string? providedCorrelationId = httpContext
            .Request.Headers[LoggingConstants.CorrelationIdHeader]
            .FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(providedCorrelationId))
        {
            return providedCorrelationId.Trim();
        }

        return httpContext.TraceIdentifier;
    }
}
