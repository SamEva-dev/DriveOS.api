using DomainRelay.DependencyInjection;
using DomainRelay.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace DriveOS.Modules.TrainingDelivery.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddTrainingDeliveryApplication(this IServiceCollection services)
    {
        services.AddDomainRelay(configureRegistration: r => r.Assemblies.Add(typeof(DependencyInjection).Assembly));
        services.AddDomainRelayValidation();
        return services;
    }
}
