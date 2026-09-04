using System.Security.Claims;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.AspNetCore.Authentication;

namespace DriveOS.Api.Security.Authentication;

internal sealed class HttpContextCurrentTenant(IHttpContextAccessor httpContextAccessor)
    : ICurrentTenant
{
    public bool HasTenant => OrganizationId is not null;

    public OrganizationId? OrganizationId =>
        TryReadResolvedGuid(
            OrganizationContextAuthorizationMiddleware.ResolvedOrganizationIdItem,
            DriveOsClaimTypes.OrganizationId
        ) is Guid value
            ? new OrganizationId(value)
            : null;

    public BranchId? BranchId =>
        TryReadResolvedGuid(
            OrganizationContextAuthorizationMiddleware.ResolvedBranchIdItem,
            DriveOsClaimTypes.BranchId
        ) is Guid value
            ? new BranchId(value)
            : null;

    private Guid? TryReadResolvedGuid(string itemKey, string claimType)
    {
        HttpContext? context = httpContextAccessor.HttpContext;
        if (context?.Items.TryGetValue(itemKey, out object? item) == true && item is Guid id)
            return id;

        string? value = context?.User.FindFirstValue(claimType);

        return Guid.TryParse(value, out Guid identifier) ? identifier : null;
    }
}
