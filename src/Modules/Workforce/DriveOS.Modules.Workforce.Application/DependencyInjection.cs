using DomainRelay.DependencyInjection;
using DomainRelay.Validation;
using Microsoft.Extensions.DependencyInjection;
namespace DriveOS.Modules.Workforce.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddWorkforceApplication(this IServiceCollection services)
    {
        services.AddDomainRelay(configureRegistration: r => r.Assemblies.Add(typeof(DependencyInjection).Assembly));
        services.AddDomainRelayValidation();
        return services;
    }
}
