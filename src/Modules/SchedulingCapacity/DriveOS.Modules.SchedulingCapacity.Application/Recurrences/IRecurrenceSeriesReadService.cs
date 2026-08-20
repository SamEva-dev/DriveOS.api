using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Application.Recurrences;

public interface IRecurrenceSeriesReadService
{
    Task<RecurrenceSeriesResponse?> GetAsync(OrganizationId organizationId, RecurrenceSeriesId seriesId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<RecurrenceSeriesResponse>> ListAsync(OrganizationId organizationId, CancellationToken cancellationToken = default);
}
