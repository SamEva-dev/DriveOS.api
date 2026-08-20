using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Application.WaitingList;

public sealed record WaitingListProposalResponse(
    Guid Id,
    DateTimeOffset StartAtUtc,
    DateTimeOffset EndAtUtc,
    Guid? BranchId,
    Guid? InstructorId,
    DateTimeOffset ProposedAtUtc,
    DateTimeOffset ExpiresAtUtc,
    int Status,
    DateTimeOffset? HeldUntilUtc,
    DateTimeOffset? DecidedAtUtc,
    string? DecisionReason,
    Guid? FulfilledBookingId);

public sealed record WaitingListEntryResponse(
    Guid Id,
    Guid StudentId,
    int RequestedSessionType,
    DateTimeOffset PreferredFromUtc,
    DateTimeOffset PreferredToUtc,
    int DurationMinutes,
    Guid? PreferredBranchId,
    Guid? PreferredInstructorId,
    int BasePriorityScore,
    int EffectivePriorityScore,
    string PriorityExplanation,
    string Reason,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset ExpiresAtUtc,
    int Status,
    IReadOnlyCollection<WaitingListProposalResponse> Proposals);

public sealed record WaitingListMatchCandidateResponse(
    Guid EntryId,
    Guid StudentId,
    int BasePriorityScore,
    int EffectivePriorityScore,
    string PriorityExplanation,
    DateTimeOffset CreatedAtUtc,
    string MatchExplanation);

public interface IWaitingListReadService
{
    Task<IReadOnlyCollection<WaitingListEntryResponse>> ListAsync(OrganizationId organizationId, int? status, Guid? studentId, CancellationToken cancellationToken = default);
    Task<WaitingListEntryResponse?> GetAsync(OrganizationId organizationId, WaitingListEntryId entryId, CancellationToken cancellationToken = default);
}

public interface IWaitingListMatchingService
{
    Task<IReadOnlyCollection<WaitingListMatchCandidateResponse>> MatchAsync(OrganizationId organizationId, DateTimeOffset startAtUtc, DateTimeOffset endAtUtc, BranchId? branchId, UserId? instructorId, int maxResults, CancellationToken cancellationToken = default);
}

public interface IWaitingListSlotLock
{
    Task AcquireAsync(OrganizationId organizationId, string slotKey, CancellationToken cancellationToken = default);
}
