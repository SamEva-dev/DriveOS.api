using DriveOS.Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace DriveOS.Api.Security.Authorization;

internal sealed class PermissionAuthorizationHandler(
    ICurrentUser currentUser)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (currentUser.IsAuthenticated
            && currentUser.HasPermission(requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
