using System.Security.Claims;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Security.Contracts;

namespace DriveOS.Api.Security.Authentication;

/// <summary>
/// Rejects organization identifiers that do not belong to the authenticated
/// context before an endpoint can reach an application handler.
/// </summary>
public sealed class OrganizationContextAuthorizationMiddleware(RequestDelegate next)
{
    public const string ResolvedOrganizationIdItem = "DriveOS.ResolvedOrganizationId";
    public const string ResolvedBranchIdItem = "DriveOS.ResolvedBranchId";

    public async Task InvokeAsync(HttpContext context, ICurrentUser currentUser)
    {
        if (!currentUser.IsAuthenticated || IsMachineEndpoint(context.Request.Path))
        {
            await next(context);
            return;
        }

        (bool organizationHeaderValid, Guid? headerOrganizationId) =
            await ReadOptionalHeaderAsync(context, "X-Organization-Id");
        if (!organizationHeaderValid)
            return;
        (bool branchHeaderValid, Guid? headerBranchId) =
            await ReadOptionalHeaderAsync(context, "X-Branch-Id");
        if (!branchHeaderValid)
            return;

        Guid? claimOrganizationId = ReadClaimGuid(context.User, DriveOsClaimTypes.OrganizationId);
        Guid? claimBranchId = ReadClaimGuid(context.User, DriveOsClaimTypes.BranchId);
        Guid? resolvedOrganizationId = headerOrganizationId ?? claimOrganizationId;
        Guid? resolvedBranchId = headerBranchId ?? claimBranchId;

        bool canReadAcrossTenants = currentUser.HasPermission(
            DriveOsPermissionCodes.Organizations.CrossTenantRead
        );
        bool canManageAcrossTenants = currentUser.HasPermission(
            DriveOsPermissionCodes.Organizations.CrossTenantManage
        );
        bool hasCrossTenantAccess = canReadAcrossTenants || canManageAcrossTenants;

        if (
            headerOrganizationId.HasValue
            && claimOrganizationId.HasValue
            && headerOrganizationId != claimOrganizationId
            && !hasCrossTenantAccess
        )
        {
            await WriteForbiddenAsync(context, "security.organization_context_not_allowed");
            return;
        }

        if (
            context.Request.RouteValues.TryGetValue("organizationId", out object? routeValue)
            && Guid.TryParse(routeValue?.ToString(), out Guid routeOrganizationId)
        )
        {
            if (!resolvedOrganizationId.HasValue && !hasCrossTenantAccess)
            {
                await WriteForbiddenAsync(context, "security.organization_context_required");
                return;
            }

            if (
                resolvedOrganizationId.HasValue
                && routeOrganizationId != resolvedOrganizationId.Value
                && !hasCrossTenantAccess
            )
            {
                await WriteForbiddenAsync(context, "security.cross_tenant_access_denied");
                return;
            }

            resolvedOrganizationId ??= routeOrganizationId;
        }

        if (resolvedOrganizationId.HasValue)
            context.Items[ResolvedOrganizationIdItem] = resolvedOrganizationId.Value;
        if (resolvedBranchId.HasValue)
            context.Items[ResolvedBranchIdItem] = resolvedBranchId.Value;

        await next(context);
    }

    private static bool IsMachineEndpoint(PathString path) =>
        path.StartsWithSegments("/api/provisioning", StringComparison.OrdinalIgnoreCase)
        || path.StartsWithSegments("/api/access-management", StringComparison.OrdinalIgnoreCase)
        || path.StartsWithSegments("/api/webhooks", StringComparison.OrdinalIgnoreCase);

    private static Guid? ReadClaimGuid(ClaimsPrincipal principal, string claimType) =>
        Guid.TryParse(principal.FindFirstValue(claimType), out Guid value) ? value : null;

    private static async Task<(bool IsValid, Guid? Value)> ReadOptionalHeaderAsync(
        HttpContext context,
        string headerName
    )
    {
        string raw = context.Request.Headers[headerName].ToString();
        if (string.IsNullOrWhiteSpace(raw))
            return (true, null);

        if (Guid.TryParse(raw, out Guid parsed) && parsed != Guid.Empty)
            return (true, parsed);

        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(
            new
            {
                type = "https://httpstatuses.com/400",
                title = "security.invalid_context_header",
                status = StatusCodes.Status400BadRequest,
                detail = $"The {headerName} header must contain a non-empty GUID.",
            }
        );
        return (false, null);
    }

    private static Task WriteForbiddenAsync(HttpContext context, string code)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.ContentType = "application/problem+json";
        return context.Response.WriteAsJsonAsync(
            new
            {
                type = "https://httpstatuses.com/403",
                title = code,
                status = StatusCodes.Status403Forbidden,
            }
        );
    }
}
