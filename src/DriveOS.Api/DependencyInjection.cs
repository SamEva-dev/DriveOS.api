using DomainRelay.Mapping.DependencyInjection.Extensions;
using DriveOS.Api.Mapping;

namespace DriveOS.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddDomainRelayMapping(
            builder =>
            {
                builder
                    .AddProfilesFromAssemblyContaining<
                        OrganizationsApiMappingProfile>()
                    .ValidateConfigurationOnBuild();
            });

        return services;
    }
}