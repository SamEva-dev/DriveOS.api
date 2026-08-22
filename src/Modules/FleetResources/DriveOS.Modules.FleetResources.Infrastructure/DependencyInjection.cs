using DriveOS.Modules.FleetResources.Application.Persistence;
using DriveOS.Modules.FleetResources.Application.Vehicles;
using DriveOS.Modules.FleetResources.Domain.Vehicles;
using DriveOS.Modules.FleetResources.Infrastructure.Persistence;
using DriveOS.Modules.FleetResources.Infrastructure.Persistence.Repositories;
using DriveOS.Modules.FleetResources.Infrastructure.Read;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DriveOS.Modules.FleetResources.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddFleetResourcesInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("DriveOS") ?? throw new InvalidOperationException("The DriveOS database connection string is missing.");
        services.AddDbContext<FleetResourcesDbContext>(options => options.UseNpgsql(connectionString, npgsql =>
            npgsql.MigrationsHistoryTable("__ef_migrations_history", "fleet_resources")));
        services.AddScoped<IFleetResourcesUnitOfWork>(sp => sp.GetRequiredService<FleetResourcesDbContext>());
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<IFleetVehicleComplianceReadService, FleetVehicleComplianceReadService>();
        return services;
    }
}
