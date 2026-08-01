using LocaGuest.Security.Contracts;
using Microsoft.AspNetCore.Authorization;

namespace DriveOS.Api.Security.Authorization;

internal static class DriveOsAuthorizationExtensions
{
    public static IServiceCollection AddDriveOsAuthorization(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddAuthorization(
            options =>
            {
                foreach (string permission in DriveOsPermissionCodes.All)
                {
                    options.AddPolicy(
                        permission,
                        policy =>
                        {
                            policy.RequireAuthenticatedUser();
                            policy.AddRequirements(
                                new PermissionRequirement(permission));
                        });
                }
            });

        services.AddScoped<
            IAuthorizationHandler,
            PermissionAuthorizationHandler>();

        return services;
    }
}
