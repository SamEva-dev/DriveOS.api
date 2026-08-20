namespace DriveOS.Modules.SchedulingCapacity.Application.SlotSearch;

public sealed class SlotSearchValidationException(
    string messageKey,
    IReadOnlyDictionary<string, object?>? parameters = null)
    : ArgumentException(messageKey)
{
    public string MessageKey { get; } = messageKey;
    public IReadOnlyDictionary<string, object?> Parameters { get; } = parameters ?? new Dictionary<string, object?>();
}
