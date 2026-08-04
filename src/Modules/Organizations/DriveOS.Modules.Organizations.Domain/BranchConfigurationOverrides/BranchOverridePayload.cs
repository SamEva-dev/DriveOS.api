using System.Text.Json;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain.BranchConfigurationOverrides;

public sealed record BranchOverridePayload
{
    public const int MaximumLength = 64_000;

    private BranchOverridePayload(string json) => Json = json;

    public string Json { get; }

    public static Result<BranchOverridePayload> Create(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return Result.Failure<BranchOverridePayload>(BranchConfigurationOverrideErrors.EmptyPayload);

        if (json.Length > MaximumLength)
            return Result.Failure<BranchOverridePayload>(BranchConfigurationOverrideErrors.PayloadTooLarge);

        try
        {
            using JsonDocument document = JsonDocument.Parse(json);

            if (document.RootElement.ValueKind != JsonValueKind.Object)
                return Result.Failure<BranchOverridePayload>(BranchConfigurationOverrideErrors.PayloadRootMustBeObject);

            // A branch override is intentionally sparse. An empty object is valid and means
            // "no local difference" while keeping a versioned, auditable draft.
            string normalized = JsonSerializer.Serialize(document.RootElement);
            return Result.Success(new BranchOverridePayload(normalized));
        }
        catch (JsonException)
        {
            return Result.Failure<BranchOverridePayload>(BranchConfigurationOverrideErrors.InvalidJson);
        }
    }
}
