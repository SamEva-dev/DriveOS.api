using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DriveOS.Api.Security;

public sealed class AuthGateMachineTokenValidator : IAuthGateMachineTokenValidator
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptionsMonitor<AuthGateMachineTokenOptions> _options;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    private IReadOnlyCollection<SecurityKey> _signingKeys = [];
    private DateTimeOffset _signingKeysExpireAtUtc = DateTimeOffset.MinValue;

    public AuthGateMachineTokenValidator(
        IHttpClientFactory httpClientFactory,
        IOptionsMonitor<AuthGateMachineTokenOptions> options
    )
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
    }

    public async Task<ClaimsPrincipal?> ValidateAsync(
        string token,
        CancellationToken cancellationToken = default,
        string? requiredClientId = null,
        string? requiredScope = null
    )
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        AuthGateMachineTokenOptions options = _options.CurrentValue;
        IReadOnlyCollection<SecurityKey> signingKeys = await GetSigningKeysAsync(
            options,
            cancellationToken
        );

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = options.Issuer,
            ValidateAudience = true,
            ValidAudience = options.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = signingKeys,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            RequireSignedTokens = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = JwtRegisteredClaimNames.Sub,
        };

        var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };

        try
        {
            ClaimsPrincipal principal = handler.ValidateToken(token, validationParameters, out _);

            string? authorizedParty = principal.FindFirst("azp")?.Value;

            string? subject = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (
                !string.Equals(
                    authorizedParty,
                    requiredClientId ?? options.RequiredClientId,
                    StringComparison.Ordinal
                )
                && !string.Equals(
                    subject,
                    requiredClientId ?? options.RequiredClientId,
                    StringComparison.Ordinal
                )
            )
            {
                return null;
            }

            string[] scopes = principal
                .FindAll("scope")
                .SelectMany(claim =>
                    claim.Value.Split(
                        ' ',
                        StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
                    )
                )
                .ToArray();

            return scopes.Contains(requiredScope ?? options.RequiredScope, StringComparer.Ordinal)
                ? principal
                : null;
        }
        catch (SecurityTokenSignatureKeyNotFoundException)
        {
            InvalidateSigningKeys();
            return await ValidateAfterKeyRefreshAsync(
                token,
                options,
                cancellationToken,
                requiredClientId,
                requiredScope
            );
        }
        catch (SecurityTokenException)
        {
            return null;
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    private async Task<ClaimsPrincipal?> ValidateAfterKeyRefreshAsync(
        string token,
        AuthGateMachineTokenOptions options,
        CancellationToken cancellationToken,
        string? requiredClientId,
        string? requiredScope
    )
    {
        IReadOnlyCollection<SecurityKey> signingKeys = await GetSigningKeysAsync(
            options,
            cancellationToken,
            forceRefresh: true
        );

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = options.Issuer,
            ValidateAudience = true,
            ValidAudience = options.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = signingKeys,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            RequireSignedTokens = true,
            ClockSkew = TimeSpan.FromSeconds(30),
        };

        var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };

        try
        {
            ClaimsPrincipal principal = handler.ValidateToken(token, validationParameters, out _);

            string? azp = principal.FindFirst("azp")?.Value;
            string? sub = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            bool validClient =
                string.Equals(
                    azp,
                    requiredClientId ?? (requiredClientId ?? options.RequiredClientId),
                    StringComparison.Ordinal
                )
                || string.Equals(
                    sub,
                    requiredClientId ?? (requiredClientId ?? options.RequiredClientId),
                    StringComparison.Ordinal
                );

            bool validScope = principal
                .FindAll("scope")
                .SelectMany(claim =>
                    claim.Value.Split(
                        ' ',
                        StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
                    )
                )
                .Contains(
                    requiredScope ?? (requiredScope ?? options.RequiredScope),
                    StringComparer.Ordinal
                );

            return validClient && validScope ? principal : null;
        }
        catch (Exception exception) when (exception is SecurityTokenException or ArgumentException)
        {
            return null;
        }
    }

    private async Task<IReadOnlyCollection<SecurityKey>> GetSigningKeysAsync(
        AuthGateMachineTokenOptions options,
        CancellationToken cancellationToken,
        bool forceRefresh = false
    )
    {
        if (
            !forceRefresh
            && _signingKeys.Count > 0
            && DateTimeOffset.UtcNow < _signingKeysExpireAtUtc
        )
        {
            return _signingKeys;
        }

        await _refreshLock.WaitAsync(cancellationToken);

        try
        {
            if (
                !forceRefresh
                && _signingKeys.Count > 0
                && DateTimeOffset.UtcNow < _signingKeysExpireAtUtc
            )
            {
                return _signingKeys;
            }

            if (string.IsNullOrWhiteSpace(options.JwksUrl))
            {
                throw new InvalidOperationException(
                    $"{AuthGateMachineTokenOptions.SectionName}:JwksUrl is missing."
                );
            }

            HttpClient client = _httpClientFactory.CreateClient("AuthGateJwks");

            string json = await client.GetStringAsync(options.JwksUrl, cancellationToken);

            var keySet = new JsonWebKeySet(json);
            IReadOnlyCollection<SecurityKey> keys = keySet.GetSigningKeys().ToArray();

            if (keys.Count == 0)
            {
                throw new InvalidOperationException(
                    "AuthGate JWKS endpoint returned no signing key."
                );
            }

            _signingKeys = keys;
            _signingKeysExpireAtUtc = DateTimeOffset.UtcNow.AddMinutes(
                Math.Max(1, options.JwksCacheMinutes)
            );

            return _signingKeys;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private void InvalidateSigningKeys()
    {
        _signingKeys = [];
        _signingKeysExpireAtUtc = DateTimeOffset.MinValue;
    }
}
