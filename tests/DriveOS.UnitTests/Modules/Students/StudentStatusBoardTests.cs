using DriveOS.Modules.Students.Domain.Statuses;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.Students;

public sealed class StudentStatusBoardTests
{
    [Fact]
    public void ApplyingBlock_ShouldNotAlterIndependentStatuses()
    {
        var board = Create();
        board.ProjectStatuses(
            FinancialStatus.UpToDate,
            PedagogicalStatus.InProgress,
            SchedulingStatus.Allowed,
            ExamStatus.Ready,
            PortalAccessStatus.Active
        );
        board.ApplyBlock(
            "MissingDocument",
            "Identity proof missing",
            "Documents",
            StudentBlockingAction.PresentExam | StudentBlockingAction.Sign,
            StudentBlockSeverity.Blocking,
            "Upload and validate document",
            UserId.New(),
            DateTimeOffset.UtcNow
        );
        board.FinancialStatus.Should().Be(FinancialStatus.UpToDate);
        board.PedagogicalStatus.Should().Be(PedagogicalStatus.InProgress);
        board.SchedulingStatus.Should().Be(SchedulingStatus.Allowed);
        board.Blocks.Single().BlockingActions.Should().HaveFlag(StudentBlockingAction.PresentExam);
    }

    [Fact]
    public void Override_ShouldBeTemporaryAndAudited()
    {
        var board = Create();
        var now = DateTimeOffset.UtcNow;
        var actor = UserId.New();
        Guid block = board
            .ApplyBlock(
                "Debt",
                "Outstanding balance",
                "Finance",
                StudentBlockingAction.Schedule,
                StudentBlockSeverity.Blocking,
                "Payment",
                actor,
                now
            )
            .Value;
        board
            .Override(block, "Manager authorization", now.AddHours(2), actor, now)
            .IsSuccess.Should()
            .BeTrue();
        board.Blocks.Single().Status.Should().Be(StudentBlockStatus.Overridden);
        board.Blocks.Single().OverrideUntilUtc.Should().Be(now.AddHours(2));
        board.History.Last().Action.Should().Be("Overridden");
    }

    [Fact]
    public void Release_ShouldResolveAnOverriddenBlockAndAuditDecision()
    {
        var board = Create();
        var now = DateTimeOffset.UtcNow;
        var actor = UserId.New();
        Guid block = board
            .ApplyBlock(
                "Debt",
                "Outstanding balance",
                "Finance",
                StudentBlockingAction.Refund,
                StudentBlockSeverity.Critical,
                "Payment",
                actor,
                now
            )
            .Value;
        board.Override(block, "Temporary approval", now.AddDays(1), actor, now);
        board
            .Release(
                block,
                StudentBlockResolutionType.Payment,
                "Payment received",
                actor,
                now.AddHours(1)
            )
            .IsSuccess.Should()
            .BeTrue();
        board.Blocks.Single().Status.Should().Be(StudentBlockStatus.Released);
        board.History.Last().Action.Should().Be("Released");
    }

    private static StudentStatusBoard Create() =>
        StudentStatusBoard.Create(OrganizationId.New(), PersonId.New()).Value;
}
