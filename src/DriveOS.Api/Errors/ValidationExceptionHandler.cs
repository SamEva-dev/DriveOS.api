using DomainRelay.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;

namespace DriveOS.Api.Errors;

public sealed class ValidationExceptionHandler
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ValidationException? validationException =
            ExtractValidationException(exception);

        if (validationException is null)
        {
            return false;
        }

        var errors = validationException.Errors
            .Select(failure =>
                new ApiErrorResponse(
                    Type: "validation",
                    Code: failure.ErrorCode,
                    MessageKey: failure.ErrorMessage,
                    Parameters: ExtractParameters(failure),
                    TraceId: httpContext.TraceIdentifier))
            .ToArray();

        httpContext.Response.StatusCode =
            StatusCodes.Status400BadRequest;

        await httpContext.Response.WriteAsJsonAsync(
            new
            {
                type = "validation",
                errors,
                traceId = httpContext.TraceIdentifier
            },
            cancellationToken);

        return true;
    }

    private static ValidationException?
        ExtractValidationException(Exception exception)
    {
        if (exception is ValidationException validationException)
        {
            return validationException;
        }

        if (exception is DomainRelayException
            {
                InnerException: ValidationException innerValidationException
            })
        {
            return innerValidationException;
        }

        return null;
    }

    private static IReadOnlyDictionary<string, object?>?
        ExtractParameters(
            FluentValidation.Results.ValidationFailure failure)
    {
        if (failure.FormattedMessagePlaceholderValues.Count == 0)
        {
            return null;
        }

        return failure.FormattedMessagePlaceholderValues
            .ToDictionary(
                item => item.Key,
                item => (object?)item.Value);
    }
}