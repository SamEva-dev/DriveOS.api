using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Availability;

public interface IAvailabilityPlanRepository
{
    Task<AvailabilityPlan?> GetByIdAsync(
        AvailabilityPlanId id,
        OrganizationId organizationId,
        CancellationToken cancellationToken = default);

    Task<AvailabilityPlan?> GetByIdForUpdateAsync(
        AvailabilityPlanId id,
        OrganizationId organizationId,
        CancellationToken cancellationToken = default);

    Task AddAsync(AvailabilityPlan availabilityPlan, CancellationToken cancellationToken = default);
}
