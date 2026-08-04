using System.Text.Json;
using System.Text.Json.Nodes;
using DriveOS.Modules.Organizations.Application.OrganizationConfigurations.Effective;

namespace DriveOS.Modules.Organizations.Infrastructure.BranchConfigurationOverrides;

internal sealed class JsonConfigurationMerger : IJsonConfigurationMerger
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = false
    };

    public string Merge(string baseJson, string overrideJson)
    {
        JsonObject baseObject = ParseObject(baseJson, nameof(baseJson));
        JsonObject overrideObject = ParseObject(overrideJson, nameof(overrideJson));

        MergeObjects(baseObject, overrideObject);
        return baseObject.ToJsonString(SerializerOptions);
    }

    private static void MergeObjects(JsonObject target, JsonObject source)
    {
        foreach ((string propertyName, JsonNode? sourceValue) in source)
        {
            if (sourceValue is JsonObject sourceObject &&
                target[propertyName] is JsonObject targetObject)
            {
                MergeObjects(targetObject, sourceObject);
                continue;
            }

            // Arrays and scalar values are replaced atomically.
            // JSON null is also an explicit local value and is preserved.
            target[propertyName] = sourceValue?.DeepClone();
        }
    }

    private static JsonObject ParseObject(string json, string parameterName)
    {
        JsonNode? node = JsonNode.Parse(json);
        return node as JsonObject
            ?? throw new ArgumentException("The configuration JSON root must be an object.", parameterName);
    }
}
