using DomainRelay.DependencyInjection;
using DomainRelay.Validation;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DriveOS.Modules.Organizations.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddOrganizationsApplication(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddDomainRelay(
            configureRegistration: registration =>
            {
                registration.Assemblies.Add(
                    typeof(DependencyInjection).Assembly);
            });

        services.AddValidatorsFromAssembly(
            typeof(DependencyInjection).Assembly);

        services.AddDomainRelayValidation();

        return services;
    }
}