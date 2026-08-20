using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Recurrences;

public static class RecurrenceSeriesErrors
{
    public static readonly Error InvalidIdentifier = Error.Validation("SchedulingCapacity.Recurrence.InvalidIdentifier", "scheduling.recurrence.invalidIdentifier");
    public static readonly Error InvalidOrganization = Error.Validation("SchedulingCapacity.Recurrence.InvalidOrganization", "scheduling.recurrence.invalidOrganization");
    public static readonly Error InvalidRule = Error.Validation("SchedulingCapacity.Recurrence.InvalidRule", "scheduling.recurrence.invalidRule");
    public static readonly Error InvalidPeriod = Error.Validation("SchedulingCapacity.Recurrence.InvalidPeriod", "scheduling.recurrence.invalidPeriod");
    public static readonly Error InvalidDuration = Error.Validation("SchedulingCapacity.Recurrence.InvalidDuration", "scheduling.recurrence.invalidDuration");
    public static readonly Error InvalidTitle = Error.Validation("SchedulingCapacity.Recurrence.InvalidTitle", "scheduling.recurrence.invalidTitle");
    public static readonly Error ResourceRequired = Error.Validation("SchedulingCapacity.Recurrence.ResourceRequired", "scheduling.recurrence.resourceRequired");
    public static readonly Error OccurrenceNotFound = Error.NotFound("SchedulingCapacity.Recurrence.OccurrenceNotFound", "scheduling.recurrence.occurrenceNotFound");
    public static readonly Error OccurrenceModificationNotAllowed = Error.Conflict("SchedulingCapacity.Recurrence.OccurrenceModificationNotAllowed", "scheduling.recurrence.occurrenceModificationNotAllowed");
    public static readonly Error SeriesCancelled = Error.Conflict("SchedulingCapacity.Recurrence.SeriesCancelled", "scheduling.recurrence.seriesCancelled");
}
