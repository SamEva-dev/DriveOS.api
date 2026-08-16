namespace DriveOS.Api.Errors;

public sealed record ApiErrorResponse(
    string Type,
    string Code,
    string MessageKey,
    IReadOnlyDictionary<string, object?>? Parameters,
    string? TraceId
);
