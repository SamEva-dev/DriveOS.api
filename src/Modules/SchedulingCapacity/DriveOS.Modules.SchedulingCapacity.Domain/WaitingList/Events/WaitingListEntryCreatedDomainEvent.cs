using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.WaitingList.Events;
public sealed record WaitingListEntryCreatedDomainEvent(WaitingListEntryId EntryId, OrganizationId OrganizationId, PersonId StudentId) : DomainEvent;
