using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Recurrences.Events;

public sealed record RecurrenceSeriesChangedDomainEvent(RecurrenceSeriesId SeriesId, OrganizationId OrganizationId, int Revision, DateOnly ApplyFrom) : DomainEvent;
