using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Text.Json;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Api.Security.Authentication;

internal sealed class HttpContextCurrentUser(
    IHttpContextAccessor httpContextAccessor)
    : ICurrentUser
{
    private readonly ClaimsPrincipal _principal =
        httpContextAccessor.HttpContext?.User
        ?? new ClaimsPrincipal();

    private IReadOnlySet<string>? _permissions;

    public bool IsAuthenticated =>
        _principal.Identity?.IsAuthenticated == true;

    public UserId? UserId
    {
        get
        {
            string? value = _principal.FindFirstValue(
                ClaimTypes.NameIdentifier)
                ?? _principal.FindFirstValue("sub");

            return Guid.TryParse(value, out Guid userId)
                ? new UserId(userId)
                : null;
        }
    }

    public string? Email =>
        _principal.FindFirstValue(ClaimTypes.Email)
        ?? _principal.FindFirstValue("email");

    public IReadOnlySet<string> Permissions =>
        _permissions ??= ReadPermissions(_principal);

    public bool HasPermission(string permission) =>
        !string.IsNullOrWhiteSpace(permission)
        && Permissions.Contains(permission);

    private static IReadOnlySet<string> ReadPermissions(
        ClaimsPrincipal principal)
    {
        var permissions = new HashSet<string>(
            StringComparer.Ordinal);

        IEnumerable<Claim> claims = principal.Claims.Where(
            claim =>
                claim.Type == DriveOsClaimTypes.Permission
                || claim.Type == DriveOsClaimTypes.Permissions);

        foreach (Claim claim in claims)
        {
            AddClaimValue(permissions, claim.Value);
        }

        return permissions;
    }

    private static void AddClaimValue(
        ISet<string> permissions,
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        string trimmed = value.Trim();

        if (trimmed.StartsWith("[", StringComparison.Ordinal))
        {
            try
            {
                string[]? values = JsonSerializer.Deserialize<string[]>(
                    trimmed);

                if (values is not null)
                {
                    foreach (string permission in values)
                    {
                        if (!string.IsNullOrWhiteSpace(permission))
                        {
                            permissions.Add(permission);
                        }
                    }
                }

                return;
            }
            catch (JsonException)
            {
                // Fall through and treat the claim as a scalar value.
            }
        }

        permissions.Add(trimmed);
    }
}
