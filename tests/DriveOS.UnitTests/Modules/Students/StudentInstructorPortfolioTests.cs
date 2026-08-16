using DriveOS.Modules.Students.Domain.Instructors;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.Students;

public sealed class StudentInstructorPortfolioTests
{
    [Fact]
    public void Assign_ShouldRejectSecondPrimary()
    {
        var p = Create();
        var now = DateTimeOffset.UtcNow;
        var today = DateOnly.FromDateTime(now.UtcDateTime);
        p.Assign(
                UserId.New(),
                StudentInstructorAssignmentType.PrimaryInstructor,
                today,
                null,
                "B",
                StudentInstructorScope.StudentRead,
                "Initial",
                UserId.New(),
                now
            )
            .IsSuccess.Should()
            .BeTrue();
        p.Assign(
                UserId.New(),
                StudentInstructorAssignmentType.PrimaryInstructor,
                today,
                null,
                "B",
                StudentInstructorScope.StudentRead,
                "Duplicate",
                UserId.New(),
                now
            )
            .Error.Should()
            .Be(StudentInstructorErrors.PrimaryAlreadyExists);
    }

    [Fact]
    public void TemporaryAssignment_ShouldExpireAccessOnSameDate()
    {
        var p = Create();
        var now = DateTimeOffset.UtcNow;
        var from = DateOnly.FromDateTime(now.UtcDateTime);
        var to = from.AddDays(7);
        var id = p.Assign(
            UserId.New(),
            StudentInstructorAssignmentType.TemporaryReplacement,
            from,
            to,
            "B",
            StudentInstructorScope.StudentRead | StudentInstructorScope.SessionsRead,
            "Absence",
            UserId.New(),
            now
        ).Value;
        p.Assignments.Single(x => x.Id == id).EffectiveTo.Should().Be(to);
        p.AccessGrants.Single(x => x.AssignmentId == id).EffectiveTo.Should().Be(to);
    }

    [Fact]
    public void ReplacePrimary_ShouldRevokeOldAccessAndRetainHistory()
    {
        var p = Create();
        var actor = UserId.New();
        var now = DateTimeOffset.UtcNow;
        var today = DateOnly.FromDateTime(now.UtcDateTime);
        var old = p.Assign(
            UserId.New(),
            StudentInstructorAssignmentType.PrimaryInstructor,
            today,
            null,
            "B",
            StudentInstructorScope.All,
            "Initial",
            actor,
            now
        ).Value;
        p.ReplacePrimary(
                UserId.New(),
                today,
                null,
                "B",
                StudentInstructorScope.All,
                "Reorganization",
                actor,
                now.AddMinutes(1)
            )
            .IsSuccess.Should()
            .BeTrue();
        p.AccessGrants.Single(x => x.AssignmentId == old).RevokedAtUtc.Should().NotBeNull();
        p.History.Should().Contain(x => x.AssignmentId == old && x.Action == "Replaced");
        p.Assignments.Should()
            .ContainSingle(x =>
                x.Type == StudentInstructorAssignmentType.PrimaryInstructor
                && x.Status == StudentInstructorAssignmentStatus.Active
            );
    }

    [Fact]
    public void End_ShouldRevokeAccessAndRetainHistory()
    {
        var p = Create();
        var actor = UserId.New();
        var now = DateTimeOffset.UtcNow;
        var today = DateOnly.FromDateTime(now.UtcDateTime);
        var id = p.Assign(
            UserId.New(),
            StudentInstructorAssignmentType.SecondaryInstructor,
            today,
            null,
            "B",
            StudentInstructorScope.PedagogyRead,
            "End mission",
            actor,
            now
        ).Value;
        p.End(id, "Mission completed", actor, now.AddDays(2)).IsSuccess.Should().BeTrue();
        p.AccessGrants.Single(x => x.AssignmentId == id).RevokedAtUtc.Should().NotBeNull();
        p.History.Should().Contain(x => x.AssignmentId == id && x.Action == "Ended");
    }

    private static StudentInstructorPortfolio Create() =>
        StudentInstructorPortfolio.Create(OrganizationId.New(), PersonId.New()).Value;
}
