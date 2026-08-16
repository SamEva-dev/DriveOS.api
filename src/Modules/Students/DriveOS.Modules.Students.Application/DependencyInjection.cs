using DomainRelay.DependencyInjection;
using DomainRelay.Validation;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DriveOS.Modules.Students.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddStudentsApplication(this IServiceCollection services)
    {
        services.AddDomainRelay(configureRegistration: registration =>
            registration.Assemblies.Add(typeof(DependencyInjection).Assembly)
        );
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddDomainRelayValidation();
        return services;
    }
}
