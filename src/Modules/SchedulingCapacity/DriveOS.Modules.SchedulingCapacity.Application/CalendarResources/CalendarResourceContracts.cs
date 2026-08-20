using DriveOS.Modules.SchedulingCapacity.Domain.CalendarResources;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Application.CalendarResources;

public sealed record CalendarResourceResponse(
    Guid Id,
    Guid? BranchId,
    string ResourceType,
    Guid ExternalResourceId,
    string DisplayName,
    int Capacity,
    string TimeZoneId,
    string Status,
    string? RestrictionReason,
    string? UnavailabilityReason,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? LastModifiedAtUtc);

public interface ICalendarResourceReadService
{
    Task<IReadOnlyCollection<CalendarResourceResponse>> ListAsync(
        OrganizationId organizationId,
        CalendarResourceType? resourceType,
        BranchId? branchId,
        CancellationToken cancellationToken = default);

    Task<CalendarResourceResponse?> GetAsync(
        OrganizationId organizationId,
        CalendarResourceId id,
        CancellationToken cancellationToken = default);
}
