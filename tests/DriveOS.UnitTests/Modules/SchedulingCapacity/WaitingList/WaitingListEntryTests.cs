using DriveOS.Modules.SchedulingCapacity.Domain.Bookings;
using DriveOS.Modules.SchedulingCapacity.Domain.WaitingList;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.UnitTests.Modules.SchedulingCapacity.WaitingList;

public sealed class WaitingListEntryTests
{
    [Fact]
    public void Matching_respects_period_branch_and_instructor_preferences()
    {
        BranchId branchId = BranchId.New();
        UserId instructorId = UserId.New();
        DateTimeOffset from = DateTimeOffset.UtcNow.AddDays(1);
        var entry = CreateEntry(from, from.AddHours(6), branchId, instructorId);

        Assert.True(entry.Matches(from.AddHours(1), from.AddHours(2), branchId, instructorId, DateTimeOffset.UtcNow));
        Assert.False(entry.Matches(from.AddHours(1), from.AddHours(2), BranchId.New(), instructorId, DateTimeOffset.UtcNow));
        Assert.False(entry.Matches(from.AddHours(1), from.AddHours(2), branchId, UserId.New(), DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Declined_proposal_is_kept_in_history_and_entry_can_match_again()
    {
        DateTimeOffset from = DateTimeOffset.UtcNow.AddDays(1);
        var entry = CreateEntry(from, from.AddHours(6), null, null);
        WaitingListProposalId proposalId = entry.Propose(from.AddHours(1), from.AddHours(2), null, null, from.AddHours(3)).Value;

        var result = entry.Decline(proposalId, "student unavailable");

        Assert.True(result.IsSuccess);
        Assert.Equal(WaitingListStatus.Declined, entry.Status);
        Assert.Single(entry.Proposals);
        Assert.Equal(WaitingListProposalStatus.Declined, entry.Proposals.Single().Status);
        Assert.Equal("student unavailable", entry.Proposals.Single().DecisionReason);
        Assert.True(entry.Matches(from.AddHours(4), from.AddHours(5), null, null, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Temporary_hold_must_expire_before_proposal_expiration()
    {
        DateTimeOffset from = DateTimeOffset.UtcNow.AddDays(1);
        var entry = CreateEntry(from, from.AddHours(6), null, null);
        WaitingListProposalId proposalId = entry.Propose(from.AddHours(1), from.AddHours(2), null, null, from.AddHours(3)).Value;

        var result = entry.Hold(proposalId, from.AddHours(4));

        Assert.True(result.IsFailure);
        Assert.Equal("SchedulingCapacity.WaitingList.HoldExpired", result.Error.Code);
    }

    [Fact]
    public void Accepted_proposal_with_booking_fulfils_entry()
    {
        DateTimeOffset from = DateTimeOffset.UtcNow.AddDays(1);
        var entry = CreateEntry(from, from.AddHours(6), null, null);
        WaitingListProposalId proposalId = entry.Propose(from.AddHours(1), from.AddHours(2), null, null, from.AddHours(3)).Value;
        BookingId bookingId = BookingId.New();

        Assert.True(entry.Accept(proposalId).IsSuccess);
        var result = entry.Fulfill(proposalId, bookingId);

        Assert.True(result.IsSuccess);
        Assert.Equal(WaitingListStatus.Fulfilled, entry.Status);
        Assert.Equal(bookingId, entry.Proposals.Single().FulfilledBookingId);
    }

    private static WaitingListEntry CreateEntry(DateTimeOffset from, DateTimeOffset to, BranchId? branchId, UserId? instructorId) =>
        WaitingListEntry.Create(
            WaitingListEntryId.New(),
            OrganizationId.New(),
            PersonId.New(),
            BookingType.TrainingSession,
            from,
            to,
            60,
            branchId,
            instructorId,
            75,
            "exam soon + limited availability",
            "no compatible slot available",
            to.AddDays(2)).Value;
}

public sealed class WaitingListPriorityPolicyTests
{
    [Fact]
    public void Priority_is_computed_from_explicit_factors_and_manual_adjustment_is_bounded()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        var result = WaitingListPriorityPolicy.Calculate(new WaitingListPriorityInput(
            now.AddDays(5), true, 14, 2, true, true, false, false, 5, "manager validated exceptional constraint"), now);

        Assert.True(result.IsSuccess);
        Assert.Equal(100, result.Value.BaseScore);
        Assert.Contains("examSoon:+30", result.Value.Explanation);
        Assert.Contains("manual:+5", result.Value.Explanation);
    }

    [Fact]
    public void Manual_adjustment_requires_an_explanation()
    {
        var result = WaitingListPriorityPolicy.Calculate(new WaitingListPriorityInput(
            null, false, 0, 0, false, false, false, false, 10, null), DateTimeOffset.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal("SchedulingCapacity.WaitingList.InvalidPriority", result.Error.Code);
    }
}
