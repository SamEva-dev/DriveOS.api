using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FleetResources.Domain.Vehicles;

public interface IVehicleRepository
{
    Task<Vehicle?> GetByIdAsync(OrganizationId organizationId, VehicleId id, CancellationToken cancellationToken = default);
    Task<Vehicle?> GetByIdForUpdateAsync(OrganizationId organizationId, VehicleId id, CancellationToken cancellationToken = default);
    Task<Vehicle?> FindByRegistrationAsync(OrganizationId organizationId, string registrationNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Vehicle>> ListAsync(OrganizationId organizationId, CancellationToken cancellationToken = default);
    void Add(Vehicle vehicle);
}
