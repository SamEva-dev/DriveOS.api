namespace DriveOS.Modules.Organizations.Application.OrganizationConfigurations.Effective;

public interface IBranchConfigurationMergePolicy
{
    BranchConfigurationMergePolicyResult Validate(string overrideJson);
}

public sealed record BranchConfigurationMergePolicyResult(
    bool IsAllowed,
    IReadOnlyCollection<string> RejectedPaths
)
{
    public static BranchConfigurationMergePolicyResult Allowed { get; } =
        new(true, Array.Empty<string>());

    public static BranchConfigurationMergePolicyResult Rejected(IEnumerable<string> paths) =>
        new(false, paths.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToArray());
}
