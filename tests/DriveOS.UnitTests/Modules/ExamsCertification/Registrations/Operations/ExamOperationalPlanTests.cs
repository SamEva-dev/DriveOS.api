using DriveOS.Modules.ExamsCertification.Domain.Registrations.Operations;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.UnitTests.Modules.ExamsCertification.Registrations.Operations;

public sealed class ExamOperationalPlanTests
{
    [Fact]
    public void RefreshFromConvocation_ShouldKeepOfficialPeriodAndBuildOperationalWindow()
    {
        DateTimeOffset now = new(2026, 8, 21, 12, 0, 0, TimeSpan.Zero);
        var created = ExamOperationalPlan.Create(
            ExamOperationalPlanId.New(), OrganizationId.New(), ExamRegistrationId.New(), PersonId.New(), UserId.New(), now);

        DateTimeOffset officialStart = now.AddDays(3).AddHours(2);
        DateTimeOffset officialEnd = officialStart.AddMinutes(32);
        DateTimeOffset meeting = officialStart.AddMinutes(-45);

        var result = created.Value.RefreshFromConvocation(
            2, officialStart, officialEnd, meeting, 15, 30, BranchId.New(), true, true, "Meet at branch",
            false, 2, 1, null, UserId.New(), now);

        Assert.True(result.IsSuccess);
        Assert.Equal(officialStart, created.Value.OfficialStartUtc);
        Assert.Equal(meeting.AddMinutes(-15), created.Value.OperationalWindowStartUtc);
        Assert.Equal(officialEnd.AddMinutes(30), created.Value.OperationalWindowEndUtc);
        Assert.Equal(ExamOperationalPlanStatus.ReadyForAssignment, created.Value.Status);
    }

    [Fact]
    public void RefreshFromConvocation_ShouldFlagMissingRequiredResource()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        var created = ExamOperationalPlan.Create(
            ExamOperationalPlanId.New(), OrganizationId.New(), ExamRegistrationId.New(), PersonId.New(), UserId.New(), now);
        DateTimeOffset start = now.AddDays(1);

        var result = created.Value.RefreshFromConvocation(
            1, start, start.AddMinutes(30), start.AddMinutes(-45), 10, 20, null, true, true, null,
            false, 0, 1, "NoInstructorAvailable", UserId.New(), now);

        Assert.True(result.IsSuccess);
        Assert.Equal(ExamOperationalPlanStatus.ConflictDetected, created.Value.Status);
    }
}
