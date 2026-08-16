using DomainRelay.Mapping.DependencyInjection.Extensions;
using DriveOS.Api.Mapping;
using DriveOS.Api.Security;
using DriveOS.Api.Security.Authentication;

namespace DriveOS.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddDriveOsAuthentication(configuration);

        services.AddDomainRelayMapping(builder =>
        {
            builder
                .AddProfilesFromAssemblyContaining<OrganizationsApiMappingProfile>()
                .ValidateConfigurationOnBuild();
        });

        services.Configure<AuthGateMachineTokenOptions>(
            configuration.GetSection(AuthGateMachineTokenOptions.SectionName)
        );

        services.AddHttpClient(
            "AuthGateJwks",
            client =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
            }
        );

        services.AddSingleton<IAuthGateMachineTokenValidator, AuthGateMachineTokenValidator>();

        return services;
    }
}
