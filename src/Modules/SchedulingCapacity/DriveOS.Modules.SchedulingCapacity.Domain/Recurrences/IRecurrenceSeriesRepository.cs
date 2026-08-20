using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Recurrences;

public interface IRecurrenceSeriesRepository
{
    Task<RecurrenceSeries?> GetByIdAsync(RecurrenceSeriesId id, OrganizationId organizationId, CancellationToken cancellationToken = default);
    Task<RecurrenceSeries?> GetByIdForUpdateAsync(RecurrenceSeriesId id, OrganizationId organizationId, CancellationToken cancellationToken = default);
    void Add(RecurrenceSeries series);
}
