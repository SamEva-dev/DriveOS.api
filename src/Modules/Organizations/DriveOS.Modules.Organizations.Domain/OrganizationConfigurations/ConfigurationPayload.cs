using System.Text.Json;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain.OrganizationConfigurations;

public sealed record ConfigurationPayload
{
    public const int MaximumLength = 200_000;

    private ConfigurationPayload(string json) => Json = json;

    public string Json { get; }

    public static Result<ConfigurationPayload> Create(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Result.Failure<ConfigurationPayload>(
                OrganizationConfigurationErrors.EmptyPayload
            );
        }

        if (json.Length > MaximumLength)
        {
            return Result.Failure<ConfigurationPayload>(
                OrganizationConfigurationErrors.PayloadTooLong(MaximumLength)
            );
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(json);

            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return Result.Failure<ConfigurationPayload>(
                    OrganizationConfigurationErrors.PayloadMustBeObject
                );
            }

            string normalized = JsonSerializer.Serialize(document.RootElement);
            return Result.Success(new ConfigurationPayload(normalized));
        }
        catch (JsonException)
        {
            return Result.Failure<ConfigurationPayload>(
                OrganizationConfigurationErrors.InvalidPayload
            );
        }
    }
}
