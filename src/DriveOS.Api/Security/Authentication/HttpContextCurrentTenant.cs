using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Api.Security.Authentication;

internal sealed class HttpContextCurrentTenant(
    IHttpContextAccessor httpContextAccessor)
    : ICurrentTenant
{
    private readonly ClaimsPrincipal _principal =
        httpContextAccessor.HttpContext?.User
        ?? new ClaimsPrincipal();

    public bool HasTenant => OrganizationId is not null;

    public OrganizationId? OrganizationId =>
        TryReadGuid(DriveOsClaimTypes.OrganizationId) is Guid value
            ? new OrganizationId(value)
            : null;

    public BranchId? BranchId =>
        TryReadGuid(DriveOsClaimTypes.BranchId) is Guid value
            ? new BranchId(value)
            : null;

    private Guid? TryReadGuid(string claimType)
    {
        string? value = _principal.FindFirstValue(claimType);

        return Guid.TryParse(value, out Guid identifier)
            ? identifier
            : null;
    }
}
