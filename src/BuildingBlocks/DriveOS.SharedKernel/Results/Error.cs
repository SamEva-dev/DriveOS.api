namespace DriveOS.SharedKernel.Results;

public sealed record Error(
    string Code,
    string MessageKey,
    ErrorType Type,
    IReadOnlyDictionary<string, object?>? Parameters = null
)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);

    public static Error Validation(
        string code,
        string messageKey,
        IReadOnlyDictionary<string, object?>? parameters = null
    ) => new(code, messageKey, ErrorType.Validation, parameters);

    public static Error NotFound(
        string code,
        string messageKey,
        IReadOnlyDictionary<string, object?>? parameters = null
    ) => new(code, messageKey, ErrorType.NotFound, parameters);

    public static Error Conflict(
        string code,
        string messageKey,
        IReadOnlyDictionary<string, object?>? parameters = null
    ) => new(code, messageKey, ErrorType.Conflict, parameters);

    public static Error Unauthorized(
        string code,
        string messageKey,
        IReadOnlyDictionary<string, object?>? parameters = null
    ) => new(code, messageKey, ErrorType.Unauthorized, parameters);

    public static Error Forbidden(
        string code,
        string messageKey,
        IReadOnlyDictionary<string, object?>? parameters = null
    ) => new(code, messageKey, ErrorType.Forbidden, parameters);

    public static Error Failure(
        string code,
        string messageKey,
        IReadOnlyDictionary<string, object?>? parameters = null
    ) => new(code, messageKey, ErrorType.Failure, parameters);
}
