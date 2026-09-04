using DriveOS.Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace DriveOS.Api.Security.Authorization;

internal sealed class PermissionAuthorizationHandler(ICurrentUser currentUser)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement
    )
    {
        bool isPlatformAdministrator =
            context.User.IsInRole("DriveOS.PlatformAdministrator")
            || context.User.IsInRole("PlatformAdmin")
            || context.User.IsInRole("SuperAdmin")
            || string.Equals(
                context.User.FindFirstValue("platform_admin"),
                "true",
                StringComparison.OrdinalIgnoreCase);

        if (currentUser.IsAuthenticated
            && (isPlatformAdministrator || currentUser.HasPermission(requirement.Permission)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
