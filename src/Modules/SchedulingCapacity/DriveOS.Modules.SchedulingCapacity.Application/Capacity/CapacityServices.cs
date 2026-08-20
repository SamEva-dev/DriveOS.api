using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Application.Capacity;

public interface ICapacityForecastService
{
    Task<CapacityForecastResponse> ForecastAsync(
        OrganizationId organizationId,
        CapacityForecastHorizon horizon,
        BranchId? branchId,
        CancellationToken cancellationToken = default);

    Task<CapacityScenarioResponse> SimulateAsync(
        OrganizationId organizationId,
        CapacityScenarioRequest request,
        CancellationToken cancellationToken = default);
}
