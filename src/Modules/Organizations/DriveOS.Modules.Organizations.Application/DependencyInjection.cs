using DomainRelay.DependencyInjection;
using DomainRelay.Validation;
using DriveOS.Modules.Organizations.Application.OrganizationSubscriptions.Access;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DriveOS.Modules.Organizations.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddOrganizationsApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddDomainRelay(configureRegistration: registration =>
        {
            registration.Assemblies.Add(typeof(DependencyInjection).Assembly);
        });

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddDomainRelayValidation();

        services.AddScoped<IOrganizationEntitlementChecker, OrganizationEntitlementChecker>();
        services.AddScoped<IOrganizationLimitChecker, OrganizationLimitChecker>();

        return services;
    }
}
