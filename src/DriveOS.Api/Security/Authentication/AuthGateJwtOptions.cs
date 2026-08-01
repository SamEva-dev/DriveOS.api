namespace DriveOS.Api.Security.Authentication;

public sealed class AuthGateJwtOptions
{
    public const string SectionName = "Authentication:AuthGate";

    public string Issuer { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;

    public string JwksUrl { get; init; } = string.Empty;

    public string RequiredClientId { get; init; } = string.Empty;

    public bool RequireHttpsMetadata { get; init; } = true;

    public int ClockSkewSeconds { get; init; } = 30;
}
