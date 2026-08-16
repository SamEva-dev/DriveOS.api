using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Errors;

public static class ApiErrorMapper
{
    public static IResult ToHttpResult(this Error error, HttpContext httpContext)
    {
        int statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,

            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,

            ErrorType.Forbidden => StatusCodes.Status403Forbidden,

            ErrorType.NotFound => StatusCodes.Status404NotFound,

            ErrorType.Conflict => StatusCodes.Status409Conflict,

            _ => StatusCodes.Status500InternalServerError,
        };

        var response = new ApiErrorResponse(
            Type: error.Type.ToString().ToLowerInvariant(),
            Code: error.Code,
            MessageKey: error.MessageKey,
            Parameters: error.Parameters,
            TraceId: httpContext.TraceIdentifier
        );

        return Results.Json(response, statusCode: statusCode);
    }
}
