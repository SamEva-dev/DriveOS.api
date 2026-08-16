using DriveOS.Modules.Students.Domain.Branches;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.Students;

public sealed class StudentBranchPortfolioTests
{
    [Fact]
    public void Assign_ShouldKeepOneIdentifiablePrimaryAndDatedSecondary()
    {
        var board = Create();
        var actor = UserId.New();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        board
            .Assign(
                BranchId.New(),
                StudentBranchAssignmentType.Primary,
                StudentBranchService.Administration,
                today,
                null,
                "Enrollment branch",
                actor,
                DateTimeOffset.UtcNow
            )
            .IsSuccess.Should()
            .BeTrue();
        board
            .Assign(
                BranchId.New(),
                StudentBranchAssignmentType.Secondary,
                StudentBranchService.TheoryCourse,
                today,
                today.AddMonths(2),
                "Theory course",
                actor,
                DateTimeOffset.UtcNow
            )
            .IsSuccess.Should()
            .BeTrue();
        board
            .Assignments.Single(x => x.Type == StudentBranchAssignmentType.Primary)
            .EffectiveTo.Should()
            .BeNull();
        board
            .Assignments.Single(x => x.Type == StudentBranchAssignmentType.Secondary)
            .EffectiveTo.Should()
            .Be(today.AddMonths(2));
    }

    [Fact]
    public void ChangePrimary_ShouldRequireFreshAnalysisAndPreserveExistingAssignments()
    {
        var board = Create();
        var actor = UserId.New();
        var now = DateTimeOffset.UtcNow;
        var today = DateOnly.FromDateTime(now.UtcDateTime);
        var old = BranchId.New();
        var target = BranchId.New();
        board.Assign(
            old,
            StudentBranchAssignmentType.Primary,
            StudentBranchService.PracticalLesson,
            today,
            null,
            "Initial",
            actor,
            now
        );
        board
            .ChangePrimary(Guid.NewGuid(), "Move", actor, now)
            .Error.Should()
            .Be(StudentBranchErrors.AnalysisRequired);
        var analysis = board.AnalyzePrimaryChange(
            target,
            [
                new BranchChangeImpact(
                    BranchImpactType.FutureSessions,
                    4,
                    "students.branches.impacts.futureSessionsReview",
                    true
                ),
            ],
            actor,
            now
        );
        board
            .ChangePrimary(analysis.Id, "Geographic mobility", actor, now.AddMinutes(1))
            .IsSuccess.Should()
            .BeTrue();
        board
            .Assignments.Should()
            .Contain(x => x.BranchId == old && x.Status == StudentBranchAssignmentStatus.Ended);
        board
            .Assignments.Should()
            .Contain(x => x.BranchId == target && x.Status == StudentBranchAssignmentStatus.Active);
        analysis.Impacts.Single().AffectedCount.Should().Be(4);
    }

    [Fact]
    public void ExpiredAnalysis_ShouldNotChangePrimary()
    {
        var board = Create();
        var actor = UserId.New();
        var now = DateTimeOffset.UtcNow;
        var today = DateOnly.FromDateTime(now.UtcDateTime);
        var old = BranchId.New();
        board.Assign(
            old,
            StudentBranchAssignmentType.Primary,
            StudentBranchService.None,
            today,
            null,
            "Initial",
            actor,
            now
        );
        var analysis = board.AnalyzePrimaryChange(BranchId.New(), [], actor, now);
        board
            .ChangePrimary(analysis.Id, "Late confirmation", actor, now.AddMinutes(31))
            .Error.Should()
            .Be(StudentBranchErrors.AnalysisExpired);
        board
            .Assignments.Single(x => x.Type == StudentBranchAssignmentType.Primary)
            .BranchId.Should()
            .Be(old);
    }

    [Fact]
    public void TemporaryInternalTransfer_ShouldScheduleTargetAndReturnToOrigin()
    {
        var board = Create();
        var actor = UserId.New();
        var now = DateTimeOffset.UtcNow;
        var today = DateOnly.FromDateTime(now.UtcDateTime);
        var source = BranchId.New();
        var target = BranchId.New();
        board.Assign(
            source,
            StudentBranchAssignmentType.Primary,
            StudentBranchService.Administration,
            today,
            null,
            "Origin",
            actor,
            now
        );
        board
            .TransferPrimary(
                target,
                today.AddDays(2),
                today.AddDays(9),
                "Temporary support",
                actor,
                now
            )
            .IsSuccess.Should()
            .BeTrue();
        board
            .Assignments.Should()
            .Contain(x =>
                x.BranchId == target
                && x.EffectiveFrom == today.AddDays(2)
                && x.EffectiveTo == today.AddDays(9)
            );
        board
            .Assignments.Should()
            .Contain(x => x.BranchId == source && x.EffectiveFrom == today.AddDays(10));
    }

    private static StudentBranchPortfolio Create() =>
        StudentBranchPortfolio.Create(OrganizationId.New(), PersonId.New()).Value;
}
