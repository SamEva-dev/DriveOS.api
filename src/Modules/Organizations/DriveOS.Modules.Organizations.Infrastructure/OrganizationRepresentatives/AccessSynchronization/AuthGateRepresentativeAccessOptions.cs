namespace DriveOS.Modules.Organizations.Infrastructure.OrganizationRepresentatives.AccessSynchronization;

public sealed class AuthGateRepresentativeAccessOptions
{
    public const string SectionName = "AuthGate:OrganizationRepresentatives";
    public bool Enabled { get; init; }
    public string SynchronizePath { get; init; } = "/internal/organization-representatives/access";
    public string RevokePath { get; init; } =
        "/internal/organization-representatives/access/revoke";
}
