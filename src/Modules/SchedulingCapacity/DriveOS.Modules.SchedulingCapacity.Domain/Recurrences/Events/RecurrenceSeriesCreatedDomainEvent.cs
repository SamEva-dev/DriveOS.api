using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Recurrences.Events;

public sealed record RecurrenceSeriesCreatedDomainEvent(RecurrenceSeriesId SeriesId, OrganizationId OrganizationId) : DomainEvent;
