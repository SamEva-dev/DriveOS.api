namespace DriveOS.Api.Security;

public sealed class AuthGateMachineTokenOptions
{
    public const string SectionName = "AuthGateMachineTokens";

    public string Issuer { get; set; } = "AuthGate";
    public string Audience { get; set; } = "DriveOS";
    public string JwksUrl { get; set; } = string.Empty;
    public string RequiredClientId { get; set; } = "authgate";
    public string RequiredScope { get; set; } = "driveos.provisioning";
    public int JwksCacheMinutes { get; set; } = 15;
}
