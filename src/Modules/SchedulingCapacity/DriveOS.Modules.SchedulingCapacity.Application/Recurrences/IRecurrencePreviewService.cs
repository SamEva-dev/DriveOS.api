using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Application.Recurrences;
public interface IRecurrencePreviewService
{
    Task<RecurrencePreviewResponse?> PreviewAsync(OrganizationId organizationId, RecurrenceSeriesId seriesId, CancellationToken cancellationToken = default);
}
