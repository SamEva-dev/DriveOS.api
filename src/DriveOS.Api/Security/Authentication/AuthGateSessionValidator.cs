using System.Net.Http.Json;

namespace DriveOS.Api.Security.Authentication;

internal sealed class AuthGateSessionValidator
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public AuthGateSessionValidator(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<bool> IsActiveAsync(string accessToken, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(accessToken)) return false;
        try
        {
            var baseUrl = (_configuration["AuthGate:BaseUrl"] ?? string.Empty).Trim().TrimEnd('/');
            if (string.IsNullOrWhiteSpace(baseUrl)) return false;
            using var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/auth/session/validate", new { accessToken }, ct);
            if (!response.IsSuccessStatusCode) return false;
            var result = await response.Content.ReadFromJsonAsync<ValidationResponse>(cancellationToken: ct);
            return result?.Active == true;
        }
        catch
        {
            return false;
        }
    }

    private sealed record ValidationResponse(bool Active);
}
