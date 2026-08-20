using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Recurrences.Events;

public sealed record RecurrenceOccurrenceChangedDomainEvent(RecurrenceSeriesId SeriesId, RecurrenceOccurrenceId OccurrenceId, OrganizationId OrganizationId, RecurrenceOccurrenceStatus Status) : DomainEvent;
