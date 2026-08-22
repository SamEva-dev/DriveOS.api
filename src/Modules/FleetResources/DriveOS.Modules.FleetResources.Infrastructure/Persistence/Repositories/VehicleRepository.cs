using DriveOS.Modules.FleetResources.Domain.Vehicles;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.FleetResources.Infrastructure.Persistence.Repositories;

internal sealed class VehicleRepository(FleetResourcesDbContext db) : IVehicleRepository
{
    public Task<Vehicle?> GetByIdAsync(OrganizationId organizationId, VehicleId id, CancellationToken cancellationToken = default) =>
        db.Vehicles.AsNoTracking().SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.Id == id, cancellationToken);
    public Task<Vehicle?> GetByIdForUpdateAsync(OrganizationId organizationId, VehicleId id, CancellationToken cancellationToken = default) =>
        db.Vehicles.SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.Id == id, cancellationToken);
    public Task<Vehicle?> FindByRegistrationAsync(OrganizationId organizationId, string registrationNumber, CancellationToken cancellationToken = default) =>
        db.Vehicles.AsNoTracking().SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.RegistrationNumber == registrationNumber.Trim().ToUpper(), cancellationToken);
    public async Task<IReadOnlyList<Vehicle>> ListAsync(OrganizationId organizationId, CancellationToken cancellationToken = default) =>
        await db.Vehicles.AsNoTracking().Where(x => x.OrganizationId == organizationId).OrderBy(x => x.RegistrationNumber).ToListAsync(cancellationToken);
    public void Add(Vehicle vehicle) => db.Vehicles.Add(vehicle);
}
