using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.WaitingList.Events;
public sealed record WaitingListSlotProposedDomainEvent(WaitingListEntryId EntryId, OrganizationId OrganizationId, WaitingListProposalId ProposalId, DateTimeOffset StartAtUtc, DateTimeOffset ExpiresAtUtc) : DomainEvent;
