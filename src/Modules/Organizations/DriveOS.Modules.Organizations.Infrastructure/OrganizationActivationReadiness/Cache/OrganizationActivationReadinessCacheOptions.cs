namespace DriveOS.Modules.Organizations.Infrastructure.OrganizationActivationReadiness.Cache;

public sealed class OrganizationActivationReadinessCacheOptions
{
    public const string SectionName = "Organizations:ActivationReadiness:Cache";

    public bool Enabled { get; set; } = true;
    public int DurationSeconds { get; set; } = 30;
}
