using DriveOS.Modules.Organizations.Domain
    .BranchAssignments;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.Organizations.BranchAssignments;

public sealed class
    BranchUserAssignmentTests
{
    [Fact]
    public void Create_ShouldCreateActiveAssignment()
    {
        DateTimeOffset now =
            DateTimeOffset.UtcNow;

        Result<BranchUserAssignment>
            result =
                BranchUserAssignment.Create(
                    BranchUserAssignmentId.New(),
                    OrganizationId.New(),
                    BranchId.New(),
                    UserId.New(),
                    BranchAssignmentRole.Instructor,
                    BranchAssignmentType.Primary,
                    now,
                    null,
                    UserId.New(),
                    now);

        result.IsSuccess
            .Should()
            .BeTrue();

        BranchUserAssignment assignment =
            result.Value;

        assignment.Status
            .Should()
            .Be(
                BranchUserAssignmentStatus
                    .Active);

        assignment.Role
            .Should()
            .Be(
                BranchAssignmentRole
                    .Instructor);

        assignment.AssignmentType
            .Should()
            .Be(
                BranchAssignmentType
                    .Primary);

        assignment.IsEffectiveAt(now)
            .Should()
            .BeTrue();
    }

    [Fact]
    public void Create_WithPlannedEndBeforeStart_ShouldFail()
    {
        DateTimeOffset now =
            DateTimeOffset.UtcNow;

        Result<BranchUserAssignment>
            result =
                BranchUserAssignment.Create(
                    BranchUserAssignmentId.New(),
                    OrganizationId.New(),
                    BranchId.New(),
                    UserId.New(),
                    BranchAssignmentRole.Secretary,
                    BranchAssignmentType.Temporary,
                    now,
                    now.AddMinutes(-1),
                    UserId.New(),
                    now);

        result.IsFailure
            .Should()
            .BeTrue();

        result.Error
            .Should()
            .Be(
                BranchUserAssignmentErrors
                    .InvalidEndDate);
    }

    [Fact]
    public void Create_WithFutureStartDate_ShouldFail()
    {
        DateTimeOffset now =
            DateTimeOffset.UtcNow;

        Result<BranchUserAssignment>
            result =
                BranchUserAssignment.Create(
                    BranchUserAssignmentId.New(),
                    OrganizationId.New(),
                    BranchId.New(),
                    UserId.New(),
                    BranchAssignmentRole.Accountant,
                    BranchAssignmentType.Secondary,
                    now.AddMinutes(1),
                    null,
                    UserId.New(),
                    now);

        result.IsFailure
            .Should()
            .BeTrue();

        result.Error
            .Should()
            .Be(
                BranchUserAssignmentErrors
                    .InvalidStartDate);
    }

    [Fact]
    public void Suspend_ShouldSuspendActiveAssignment()
    {
        DateTimeOffset now =
            DateTimeOffset.UtcNow;

        BranchUserAssignment assignment =
            CreateAssignment(now);

        BranchAssignmentReason reason =
            BranchAssignmentReason
                .Create(
                    "Absence temporaire.")
                .Value;

        Result result =
            assignment.Suspend(
                reason,
                UserId.New(),
                now.AddHours(1));

        result.IsSuccess
            .Should()
            .BeTrue();

        assignment.Status
            .Should()
            .Be(
                BranchUserAssignmentStatus
                    .Suspended);

        assignment.SuspensionReason
            .Should()
            .Be(
                "Absence temporaire.");

        assignment.IsEffectiveAt(
                now.AddHours(1))
            .Should()
            .BeFalse();
    }

    [Fact]
    public void Suspend_WhenAlreadySuspended_ShouldFail()
    {
        DateTimeOffset now =
            DateTimeOffset.UtcNow;

        BranchUserAssignment assignment =
            CreateAssignment(now);

        BranchAssignmentReason reason =
            BranchAssignmentReason
                .Create(
                    "Suspension.")
                .Value;

        assignment.Suspend(
            reason,
            UserId.New(),
            now.AddMinutes(10));

        Result secondResult =
            assignment.Suspend(
                reason,
                UserId.New(),
                now.AddMinutes(20));

        secondResult.IsFailure
            .Should()
            .BeTrue();

        secondResult.Error
            .Should()
            .Be(
                BranchUserAssignmentErrors
                    .AlreadySuspended);
    }

    [Fact]
    public void Reactivate_ShouldReactivateSuspendedAssignment()
    {
        DateTimeOffset now =
            DateTimeOffset.UtcNow;

        BranchUserAssignment assignment =
            CreateAssignment(now);

        BranchAssignmentReason suspensionReason =
            BranchAssignmentReason
                .Create(
                    "Absence.")
                .Value;

        assignment.Suspend(
            suspensionReason,
            UserId.New(),
            now.AddMinutes(10));

        BranchAssignmentReason
            reactivationReason =
                BranchAssignmentReason
                    .Create(
                        "Retour dans l’agence.")
                    .Value;

        Result result =
            assignment.Reactivate(
                reactivationReason,
                UserId.New(),
                now.AddHours(1));

        result.IsSuccess
            .Should()
            .BeTrue();

        assignment.Status
            .Should()
            .Be(
                BranchUserAssignmentStatus
                    .Active);

        assignment.SuspensionReason
            .Should()
            .BeNull();

        assignment.SuspendedAtUtc
            .Should()
            .BeNull();
    }

    [Fact]
    public void Reactivate_WhenAssignmentIsActive_ShouldFail()
    {
        DateTimeOffset now =
            DateTimeOffset.UtcNow;

        BranchUserAssignment assignment =
            CreateAssignment(now);

        BranchAssignmentReason reason =
            BranchAssignmentReason
                .Create(
                    "Réactivation.")
                .Value;

        Result result =
            assignment.Reactivate(
                reason,
                UserId.New(),
                now.AddMinutes(10));

        result.IsFailure
            .Should()
            .BeTrue();

        result.Error
            .Should()
            .Be(
                BranchUserAssignmentErrors
                    .NotSuspended);
    }

    [Fact]
    public void End_ShouldEndActiveAssignment()
    {
        DateTimeOffset now =
            DateTimeOffset.UtcNow;

        BranchUserAssignment assignment =
            CreateAssignment(now);

        DateTimeOffset endedAt =
            now.AddHours(2);

        BranchAssignmentReason reason =
            BranchAssignmentReason
                .Create(
                    "Fin de collaboration.")
                .Value;

        Result result =
            assignment.End(
                reason,
                endedAt,
                UserId.New(),
                endedAt);

        result.IsSuccess
            .Should()
            .BeTrue();

        assignment.Status
            .Should()
            .Be(
                BranchUserAssignmentStatus
                    .Ended);

        assignment.EffectiveEndAtUtc
            .Should()
            .Be(endedAt);

        assignment.EndReason
            .Should()
            .Be(
                "Fin de collaboration.");

        assignment.IsEffectiveAt(endedAt)
            .Should()
            .BeFalse();
    }

    [Fact]
    public void End_WithDateBeforeStart_ShouldFail()
    {
        DateTimeOffset now =
            DateTimeOffset.UtcNow;

        BranchUserAssignment assignment =
            CreateAssignment(now);

        BranchAssignmentReason reason =
            BranchAssignmentReason
                .Create(
                    "Correction.")
                .Value;

        Result result =
            assignment.End(
                reason,
                now.AddMinutes(-1),
                UserId.New(),
                now);

        result.IsFailure
            .Should()
            .BeTrue();

        result.Error
            .Should()
            .Be(
                BranchUserAssignmentErrors
                    .InvalidEndDate);
    }

    [Fact]
    public void End_WhenAlreadyEnded_ShouldFail()
    {
        DateTimeOffset now =
            DateTimeOffset.UtcNow;

        BranchUserAssignment assignment =
            CreateAssignment(now);

        BranchAssignmentReason reason =
            BranchAssignmentReason
                .Create(
                    "Fin.")
                .Value;

        assignment.End(
            reason,
            now.AddHours(1),
            UserId.New(),
            now.AddHours(1));

        Result secondResult =
            assignment.End(
                reason,
                now.AddHours(2),
                UserId.New(),
                now.AddHours(2));

        secondResult.IsFailure
            .Should()
            .BeTrue();

        secondResult.Error
            .Should()
            .Be(
                BranchUserAssignmentErrors
                    .AlreadyEnded);
    }

    [Fact]
    public void End_ShouldAlsoEndSuspendedAssignment()
    {
        DateTimeOffset now =
            DateTimeOffset.UtcNow;

        BranchUserAssignment assignment =
            CreateAssignment(now);

        BranchAssignmentReason
            suspensionReason =
                BranchAssignmentReason
                    .Create(
                        "Suspension.")
                    .Value;

        assignment.Suspend(
            suspensionReason,
            UserId.New(),
            now.AddMinutes(10));

        BranchAssignmentReason endReason =
            BranchAssignmentReason
                .Create(
                    "Départ définitif.")
                .Value;

        Result result =
            assignment.End(
                endReason,
                now.AddHours(1),
                UserId.New(),
                now.AddHours(1));

        result.IsSuccess
            .Should()
            .BeTrue();

        assignment.Status
            .Should()
            .Be(
                BranchUserAssignmentStatus
                    .Ended);

        assignment.SuspensionReason
            .Should()
            .BeNull();
    }

    private static BranchUserAssignment
        CreateAssignment(
            DateTimeOffset now)
    {
        Result<BranchUserAssignment>
            result =
                BranchUserAssignment.Create(
                    BranchUserAssignmentId.New(),
                    OrganizationId.New(),
                    BranchId.New(),
                    UserId.New(),
                    BranchAssignmentRole.Instructor,
                    BranchAssignmentType.Primary,
                    now,
                    null,
                    UserId.New(),
                    now);

        return result.Value;
    }
}