namespace DriveOS.Modules.Organizations.Infrastructure.OrganizationRepresentatives.Expiration;

public sealed class OrganizationRepresentativeExpirationOptions
{
    public const string SectionName = "OrganizationRepresentatives:Expiration";
    public bool Enabled { get; init; } = true;
    public int IntervalMinutes { get; init; } = 60;
    public int BatchSize { get; init; } = 100;
}
