using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.Modules.SchedulingCapacity.Domain.Bookings;
using DriveOS.Modules.SchedulingCapacity.Domain.WaitingList;

namespace DriveOS.Modules.SchedulingCapacity.Application.WaitingList;

public sealed record CreateWaitingListEntryCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    BookingType RequestedSessionType,
    DateTimeOffset PreferredFromUtc,
    DateTimeOffset PreferredToUtc,
    int DurationMinutes,
    BranchId? PreferredBranchId,
    UserId? PreferredInstructorId,
    WaitingListPriorityInput Priority,
    string Reason,
    DateTimeOffset ExpiresAtUtc) : ICommand<WaitingListEntryId>;

public sealed record UpdateWaitingListPreferencesCommand(
    OrganizationId OrganizationId,
    WaitingListEntryId EntryId,
    DateTimeOffset PreferredFromUtc,
    DateTimeOffset PreferredToUtc,
    BranchId? PreferredBranchId,
    UserId? PreferredInstructorId,
    DateTimeOffset ExpiresAtUtc) : ICommand;

public sealed record ProposeWaitingListSlotCommand(
    OrganizationId OrganizationId,
    WaitingListEntryId EntryId,
    DateTimeOffset StartAtUtc,
    DateTimeOffset EndAtUtc,
    BranchId? BranchId,
    UserId? InstructorId,
    DateTimeOffset ExpiresAtUtc) : ICommand<WaitingListProposalId>;

public sealed record HoldWaitingListProposalCommand(
    OrganizationId OrganizationId,
    WaitingListEntryId EntryId,
    WaitingListProposalId ProposalId,
    DateTimeOffset HeldUntilUtc) : ICommand;

public sealed record AcceptWaitingListProposalCommand(
    OrganizationId OrganizationId,
    WaitingListEntryId EntryId,
    WaitingListProposalId ProposalId) : ICommand;

public sealed record FulfillWaitingListEntryCommand(
    OrganizationId OrganizationId,
    WaitingListEntryId EntryId,
    WaitingListProposalId ProposalId,
    BookingId BookingId) : ICommand;

public sealed record DeclineWaitingListProposalCommand(
    OrganizationId OrganizationId,
    WaitingListEntryId EntryId,
    WaitingListProposalId ProposalId,
    string? Reason) : ICommand;

public sealed record CancelWaitingListEntryCommand(
    OrganizationId OrganizationId,
    WaitingListEntryId EntryId,
    string Reason) : ICommand;
