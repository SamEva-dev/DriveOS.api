using DriveOS.Modules.SchedulingCapacity.Application.WaitingList;
using DriveOS.Modules.SchedulingCapacity.Domain.WaitingList;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Read;

internal sealed class WaitingListMatchingService(IWaitingListEntryRepository repository) : IWaitingListMatchingService
{
    public async Task<IReadOnlyCollection<WaitingListMatchCandidateResponse>> MatchAsync(
        OrganizationId organizationId,
        DateTimeOffset startAtUtc,
        DateTimeOffset endAtUtc,
        BranchId? branchId,
        UserId? instructorId,
        int maxResults,
        CancellationToken cancellationToken = default)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        IReadOnlyCollection<WaitingListEntry> source = await repository.GetCandidatesAsync(organizationId, startAtUtc.ToUniversalTime(), endAtUtc.ToUniversalTime(), cancellationToken);
        return source
            .Where(x => x.Matches(startAtUtc, endAtUtc, branchId, instructorId, now))
            .OrderByDescending(x => WaitingListPriorityPolicy.CalculateEffectiveScore(x.PriorityScore, x.CreatedAtUtc, now))
            .ThenBy(x => x.CreatedAtUtc)
            .Take(Math.Clamp(maxResults, 1, 100))
            .Select(x => new WaitingListMatchCandidateResponse(
                x.Id.Value,
                x.StudentId.Value,
                x.PriorityScore,
                WaitingListPriorityPolicy.CalculateEffectiveScore(x.PriorityScore, x.CreatedAtUtc, now),
                x.PriorityExplanation,
                x.CreatedAtUtc,
                BuildExplanation(x, branchId, instructorId)))
            .ToArray();
    }

    private static string BuildExplanation(WaitingListEntry entry, BranchId? branchId, UserId? instructorId)
    {
        int effective = WaitingListPriorityPolicy.CalculateEffectiveScore(entry.PriorityScore, entry.CreatedAtUtc, DateTimeOffset.UtcNow);
        int aging = effective - entry.PriorityScore;
        List<string> factors = [$"basePriority:{entry.PriorityScore}", $"aging:+{aging}", $"effectivePriority:{effective}", "period:compatible", "duration:compatible"];
        if (entry.PreferredBranchId.HasValue && entry.PreferredBranchId == branchId) factors.Add("branch:preferred");
        if (entry.PreferredInstructorId.HasValue && entry.PreferredInstructorId == instructorId) factors.Add("instructor:preferred");
        factors.Add($"waitingSince:{entry.CreatedAtUtc:O}");
        return string.Join(';', factors);
    }
}
