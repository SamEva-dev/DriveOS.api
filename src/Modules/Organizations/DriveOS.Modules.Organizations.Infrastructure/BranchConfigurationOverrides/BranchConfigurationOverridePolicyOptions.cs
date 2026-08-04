namespace DriveOS.Modules.Organizations.Infrastructure.BranchConfigurationOverrides;

internal sealed class BranchConfigurationOverridePolicyOptions
{
    public const string SectionName = "OrganizationConfigurations:BranchOverrides";

    public string[] AllowedPaths { get; init; } =
    [
        "booking",
        "scheduling",
        "openingHours",
        "training.sessionDefaults",
        "communication",
        "localization"
    ];

    public string[] LockedPaths { get; init; } =
    [
        "compliance",
        "regulation",
        "security",
        "identity",
        "billing.tax"
    ];
}
