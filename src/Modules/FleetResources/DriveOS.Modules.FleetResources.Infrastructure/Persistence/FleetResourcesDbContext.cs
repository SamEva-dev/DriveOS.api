using DriveOS.Modules.FleetResources.Application.Persistence;
using DriveOS.Modules.FleetResources.Domain.Vehicles;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.FleetResources.Infrastructure.Persistence;

public sealed class FleetResourcesDbContext(DbContextOptions<FleetResourcesDbContext> options) : DbContext(options), IFleetResourcesUnitOfWork
{
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(FleetResourcesSchema.Name);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FleetResourcesDbContext).Assembly);
    }
    public Task<int> CommitAsync(CancellationToken cancellationToken = default) => SaveChangesAsync(cancellationToken);
}
