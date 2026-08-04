using DomainRelay.DependencyInjection;
using DomainRelay.Validation;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using DriveOS.Modules.Organizations.Application.OrganizationSubscriptions.Access;

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

        services.AddScoped<IOrganizationEntitlementChecker, OrganizationEntitlementChecker>();
        services.AddScoped<IOrganizationLimitChecker, OrganizationLimitChecker>();

        return services;
    }
}