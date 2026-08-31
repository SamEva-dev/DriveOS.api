using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.CalendarResources;

public interface ICalendarResourceRepository
{
    Task<CalendarResource?> GetByIdAsync(
        CalendarResourceId id,
        OrganizationId organizationId,
        CancellationToken cancellationToken = default);

    Task<CalendarResource?> GetByIdForUpdateAsync(
        CalendarResourceId id,
        OrganizationId organizationId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByExternalReferenceAsync(
        OrganizationId organizationId,
        CalendarResourceType resourceType,
        Guid externalResourceId,
        CancellationToken cancellationToken = default);

    Task<CalendarResource?> GetByExternalReferenceAsync(
        OrganizationId organizationId,
        CalendarResourceType resourceType,
        Guid externalResourceId,
        CancellationToken cancellationToken = default);

    Task<CalendarResource?> GetByExternalReferenceForUpdateAsync(
        OrganizationId organizationId,
        CalendarResourceType resourceType,
        Guid externalResourceId,
        CancellationToken cancellationToken = default);

    void Add(CalendarResource resource);
}
