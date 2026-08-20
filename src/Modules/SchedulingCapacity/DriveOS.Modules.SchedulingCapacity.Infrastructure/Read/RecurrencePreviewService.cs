using DriveOS.Modules.SchedulingCapacity.Application.Bookings;
using DriveOS.Modules.SchedulingCapacity.Application.Recurrences;
using DriveOS.Modules.SchedulingCapacity.Domain.Bookings;
using DriveOS.Modules.SchedulingCapacity.Domain.Recurrences;
using DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Read;

internal sealed class RecurrencePreviewService(SchedulingCapacityDbContext db, IBookingConflictAssessmentService conflicts) : IRecurrencePreviewService
{
    public async Task<RecurrencePreviewResponse?> PreviewAsync(OrganizationId organizationId, RecurrenceSeriesId seriesId, CancellationToken cancellationToken = default)
    {
        RecurrenceSeries? series = await db.RecurrenceSeries.AsNoTracking().Include(x=>x.Occurrences).Include(x=>x.Resources)
            .SingleOrDefaultAsync(x=>x.OrganizationId==organizationId && x.Id==seriesId,cancellationToken);
        if (series is null) return null;
        var previews = new List<RecurrenceOccurrencePreviewResponse>();
        foreach (RecurrenceOccurrence occurrence in series.Occurrences.Where(x=>x.Status is RecurrenceOccurrenceStatus.Planned or RecurrenceOccurrenceStatus.Rescheduled).OrderBy(x=>x.StartAtUtc))
        {
            if (series.ResourceSelectionPolicy == ResourceSelectionPolicy.BestAvailableResources)
            {
                previews.Add(new(occurrence.Id.Value, occurrence.StartAtUtc, occurrence.EndAtUtc, occurrence.Status.ToString(), occurrence.ExceptionReason, true, []));
                continue;
            }
            var created = Booking.Create(BookingId.New(), organizationId, series.BranchId, ResolveBookingType(series.TargetType), occurrence.StartAtUtc, occurrence.EndAtUtc, series.Title);
            if (created.IsFailure) { previews.Add(new(occurrence.Id.Value, occurrence.StartAtUtc, occurrence.EndAtUtc, occurrence.Status.ToString(), occurrence.ExceptionReason, false, [created.Error.Code])); continue; }
            foreach (RecurrenceResource resource in series.Resources)
                created.Value.AddResource(BookingResourceId.New(),resource.CalendarResourceId,resource.Quantity);
            BookingConflictAssessment assessment = await conflicts.AssessAsync(created.Value,cancellationToken);
            previews.Add(new(occurrence.Id.Value, occurrence.StartAtUtc, occurrence.EndAtUtc, occurrence.Status.ToString(), occurrence.ExceptionReason, assessment.IsConflictFree, assessment.Conflicts.Select(x => x.Type.ToString()).Distinct().ToArray()));
        }
        int conflicting = previews.Count(x=>!x.IsConflictFree);
        int exceptions = series.Occurrences.Count(x => x.Status is RecurrenceOccurrenceStatus.Cancelled or RecurrenceOccurrenceStatus.Rescheduled or RecurrenceOccurrenceStatus.Superseded);
        return new(series.Id.Value, previews.Count, previews.Count - conflicting, conflicting, exceptions, previews);
    }

    private static BookingType ResolveBookingType(RecurrenceTargetType type) => type switch
    {
        RecurrenceTargetType.Maintenance => BookingType.Maintenance,
        RecurrenceTargetType.Meeting => BookingType.Meeting,
        _ => BookingType.TrainingSession
    };
}
