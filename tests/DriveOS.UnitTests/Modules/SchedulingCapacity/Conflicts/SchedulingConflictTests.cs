using DriveOS.Modules.SchedulingCapacity.Domain.Conflicts;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.UnitTests.Modules.SchedulingCapacity.Conflicts;

public sealed class SchedulingConflictTests
{
    [Fact]
    public void Critical_conflict_cannot_be_overridden()
    {
        var conflict = SchedulingConflict.Create(
            SchedulingConflictId.New(), OrganizationId.New(), BookingId.New(), CalendarResourceId.New(), null,
            SchedulingConflictType.ResourceUnavailable, SchedulingConflictPriority.Critical,
            "scheduling.conflicts.ResourceUnavailable", "maintenance", [SchedulingConflictResolution.Reschedule]).Value;

        var result = conflict.Override("exception", "safety risk", UserId.New(), DateTimeOffset.UtcNow.AddHours(1));

        Assert.True(result.IsFailure);
        Assert.Equal("SchedulingCapacity.Conflict.CriticalOverrideForbidden", result.Error.Code);
    }

    [Fact]
    public void Non_critical_override_requires_reason_risk_approver_and_expiry()
    {
        var conflict = SchedulingConflict.Create(
            SchedulingConflictId.New(), OrganizationId.New(), BookingId.New(), CalendarResourceId.New(), null,
            SchedulingConflictType.TravelTimeConflict, SchedulingConflictPriority.High,
            "scheduling.conflicts.TravelTimeConflict", "gap=10", [SchedulingConflictResolution.AcceptRiskWithReason]).Value;

        var result = conflict.Override("traffic known", "late start possible", UserId.New(), DateTimeOffset.UtcNow.AddHours(2));

        Assert.True(result.IsSuccess);
        Assert.Equal(SchedulingConflictStatus.Overridden, conflict.Status);
        Assert.NotNull(conflict.OverrideExpiresAtUtc);
    }

    [Fact]
    public void Resolution_closes_conflict_but_keeps_resolution_reason()
    {
        var conflict = SchedulingConflict.Create(
            SchedulingConflictId.New(), OrganizationId.New(), BookingId.New(), null, null,
            SchedulingConflictType.FinancialRestriction, SchedulingConflictPriority.High,
            "scheduling.conflicts.FinancialRestriction", null, [SchedulingConflictResolution.RequestDecision]).Value;

        var result = conflict.Resolve(SchedulingConflictResolution.RequestDecision, "validated by finance", UserId.New());

        Assert.True(result.IsSuccess);
        Assert.Equal(SchedulingConflictStatus.Resolved, conflict.Status);
        Assert.Equal("validated by finance", conflict.ResolutionReason);
    }

    [Fact]
    public void Expired_override_reopens_conflict_without_erasing_override_audit_data()
    {
        var conflict = SchedulingConflict.Create(
            SchedulingConflictId.New(), OrganizationId.New(), BookingId.New(), CalendarResourceId.New(), null,
            SchedulingConflictType.TravelTimeConflict, SchedulingConflictPriority.High,
            "scheduling.conflicts.TravelTimeConflict", "gap=10", [SchedulingConflictResolution.AcceptRiskWithReason]).Value;

        DateTimeOffset now = DateTimeOffset.UtcNow;
        conflict.Override("exception approved", "late start possible", UserId.New(), now.AddMinutes(10));

        bool changed = conflict.RefreshExpiredOverride(now.AddMinutes(11));

        Assert.True(changed);
        Assert.Equal(SchedulingConflictStatus.Open, conflict.Status);
        Assert.Equal("exception approved", conflict.OverrideReason);
        Assert.Equal("late start possible", conflict.OverrideRisk);
    }
}
