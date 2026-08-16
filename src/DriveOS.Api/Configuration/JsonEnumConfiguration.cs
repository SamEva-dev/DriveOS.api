using System.Text.Json.Serialization;

namespace DriveOS.Api.Configuration;

public static class JsonEnumConfiguration
{
    /// <summary>
    /// Keeps the API contract aligned with Angular, which sends and receives enum names.
    /// Add this converter once to the HTTP JSON configuration.
    /// </summary>
    public static void ConfigureDriveOsEnums(
        this Microsoft.AspNetCore.Http.Json.JsonOptions options
    )
    {
        if (!options.SerializerOptions.Converters.OfType<JsonStringEnumConverter>().Any())
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        }
    }
}
