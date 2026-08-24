using System.Net.Http.Headers;
using System.Net.Http.Json;
using DriveOS.Modules.Workforce.Application.Offboarding;
using DriveOS.Modules.Workforce.Domain.Offboarding;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DriveOS.Modules.Workforce.Infrastructure.AccessRevocation;

public sealed class AuthGateWorkforceAccessOptions
{
    public const string SectionName = "AuthGate:WorkforceAccess";
    public bool Enabled { get; set; }
    public string TokenPath { get; set; } = "/api/m2m/token";
    public string RevokePath { get; set; } = "/internal/application-memberships/revoke";
    public string ClientId { get; set; } = "driveos-api";
    public string ClientSecret { get; set; } = string.Empty;
    public string Scope { get; set; } = "driveos.access-management";
    public string AppId { get; set; } = "driveos-web";
}

internal sealed class AuthGateEmployeeApplicationAccessRevoker(
    HttpClient httpClient,
    IOptions<AuthGateWorkforceAccessOptions> options,
    ILogger<AuthGateEmployeeApplicationAccessRevoker> logger) : IEmployeeApplicationAccessRevoker
{
    private readonly AuthGateWorkforceAccessOptions _options = options.Value;

    public async Task<Result> RevokeAsync(
        OrganizationId organizationId,
        UserId userId,
        string reason,
        CancellationToken ct = default)
    {
        if (!_options.Enabled || string.IsNullOrWhiteSpace(_options.ClientSecret))
            return Result.Failure(OffboardingErrors.AccessRevocationUnavailable);

        using HttpResponseMessage tokenResponse = await httpClient.PostAsJsonAsync(
            _options.TokenPath,
            new { clientId = _options.ClientId, clientSecret = _options.ClientSecret, scope = _options.Scope },
            ct);

        if (!tokenResponse.IsSuccessStatusCode)
        {
            logger.LogError("AuthGate M2M token acquisition failed with status {StatusCode}.", tokenResponse.StatusCode);
            return Result.Failure(OffboardingErrors.AccessRevocationFailed);
        }

        var token = await tokenResponse.Content.ReadFromJsonAsync<MachineTokenResponse>(cancellationToken: ct);
        if (token is null || string.IsNullOrWhiteSpace(token.AccessToken))
            return Result.Failure(OffboardingErrors.AccessRevocationFailed);

        using HttpRequestMessage revokeRequest = new(HttpMethod.Post, _options.RevokePath)
        {
            Content = JsonContent.Create(new
            {
                userId = userId.Value,
                appId = _options.AppId,
                organizationId = organizationId.Value,
                reason = string.IsNullOrWhiteSpace(reason) ? "Employment ended" : reason.Trim()
            })
        };
        revokeRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

        using HttpResponseMessage revokeResponse = await httpClient.SendAsync(revokeRequest, ct);
        if (!revokeResponse.IsSuccessStatusCode)
        {
            logger.LogError(
                "AuthGate application access revocation failed for user {UserId}, organization {OrganizationId}; status {StatusCode}.",
                userId.Value,
                organizationId.Value,
                revokeResponse.StatusCode);
            return Result.Failure(OffboardingErrors.AccessRevocationFailed);
        }

        return Result.Success();
    }

    private sealed record MachineTokenResponse(string AccessToken, int ExpiresIn);
}
