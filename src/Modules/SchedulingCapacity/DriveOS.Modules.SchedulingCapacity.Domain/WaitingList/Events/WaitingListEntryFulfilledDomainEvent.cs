using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.WaitingList.Events;
public sealed record WaitingListEntryFulfilledDomainEvent(WaitingListEntryId EntryId, OrganizationId OrganizationId, BookingId BookingId) : DomainEvent;
