using System.Security.Claims;

namespace DriveOS.Api.Security;

public interface IAuthGateMachineTokenValidator
{
    Task<ClaimsPrincipal?> ValidateAsync(
        string token,
        CancellationToken cancellationToken = default,
        string? requiredClientId = null,
        string? requiredScope = null
    );
}
