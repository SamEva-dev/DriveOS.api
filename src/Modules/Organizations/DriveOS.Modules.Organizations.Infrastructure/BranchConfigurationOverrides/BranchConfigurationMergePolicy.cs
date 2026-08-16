using System.Text.Json;
using DriveOS.Modules.Organizations.Application.OrganizationConfigurations.Effective;
using Microsoft.Extensions.Options;

namespace DriveOS.Modules.Organizations.Infrastructure.BranchConfigurationOverrides;

internal sealed class BranchConfigurationMergePolicy(
    IOptions<BranchConfigurationOverridePolicyOptions> options
) : IBranchConfigurationMergePolicy
{
    private readonly string[] _allowedPaths = Normalize(options.Value.AllowedPaths);
    private readonly string[] _lockedPaths = Normalize(options.Value.LockedPaths);

    public BranchConfigurationMergePolicyResult Validate(string overrideJson)
    {
        using JsonDocument document = JsonDocument.Parse(overrideJson);
        var rejected = new List<string>();

        Visit(document.RootElement, null, rejected);
        return rejected.Count == 0
            ? BranchConfigurationMergePolicyResult.Allowed
            : BranchConfigurationMergePolicyResult.Rejected(rejected);
    }

    private void Visit(JsonElement element, string? parentPath, ICollection<string> rejected)
    {
        if (element.ValueKind != JsonValueKind.Object)
            return;

        foreach (JsonProperty property in element.EnumerateObject())
        {
            string path = string.IsNullOrWhiteSpace(parentPath)
                ? property.Name
                : $"{parentPath}.{property.Name}";

            if (IsLocked(path) || !IsAllowed(path))
                rejected.Add(path);

            if (property.Value.ValueKind == JsonValueKind.Object)
                Visit(property.Value, path, rejected);
        }
    }

    private bool IsLocked(string path) =>
        _lockedPaths.Any(locked => IsSameOrDescendant(path, locked));

    private bool IsAllowed(string path) =>
        _allowedPaths.Any(allowed =>
            IsSameOrDescendant(path, allowed) || IsSameOrDescendant(allowed, path)
        );

    private static bool IsSameOrDescendant(string candidate, string root) =>
        candidate.Equals(root, StringComparison.OrdinalIgnoreCase)
        || candidate.StartsWith(root + ".", StringComparison.OrdinalIgnoreCase);

    private static string[] Normalize(IEnumerable<string>? paths) =>
        (paths ?? Array.Empty<string>())
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(path => path.Trim().Trim('.'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
}
