using DriveOS.Modules.Students.Domain.Closures;
using DriveOS.Modules.Students.Domain.Enrollments;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.Students;

public sealed class EnrollmentClosureCaseTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 16, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Closure_requires_every_precondition_exactly_once()
    {
        var result = Create(Checks().Skip(1).ToArray());
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EnrollmentClosureErrors.InvalidChecks);
    }

    [Fact]
    public void Other_reason_requires_detail()
    {
        var result = EnrollmentClosureCase.Create(
            OrganizationId.New(),
            PersonId.New(),
            DraftEnrollmentId.New(),
            EnrollmentStatus.Active,
            EnrollmentClosureReason.Other,
            DateOnly.FromDateTime(Now.UtcDateTime),
            "",
            UserId.New(),
            Now,
            Checks()
        );
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EnrollmentClosureErrors.InvalidReason);
    }

    [Fact]
    public void Closing_and_archiving_are_distinct_transitions()
    {
        var x = Create(Checks()).Value;
        x.Status.Should().Be(EnrollmentClosureStatus.ReadyToClose);
        x.Close(Guid.NewGuid(), UserId.New(), Now).IsSuccess.Should().BeTrue();
        x.Status.Should().Be(EnrollmentClosureStatus.Closed);
        x.Archive(
                DateOnly.FromDateTime(Now.AddYears(5).UtcDateTime),
                "Legal retention obligation",
                StudentDataRetentionScope.Identity | StudentDataRetentionScope.Audit,
                UserId.New(),
                Now
            )
            .IsSuccess.Should()
            .BeTrue();
        x.Status.Should().Be(EnrollmentClosureStatus.Archived);
    }

    [Fact]
    public void Blocking_check_prevents_closure()
    {
        var checks = Checks();
        checks[0] = new(
            EnrollmentClosureCheckType.FutureSessions,
            EnrollmentClosureCheckStatus.Blocking,
            "Future booking exists"
        );
        var x = Create(checks).Value;
        x.Close(Guid.NewGuid(), UserId.New(), Now).IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Reopening_requires_a_meaningful_justification()
    {
        var x = Create(Checks()).Value;
        x.Close(Guid.NewGuid(), UserId.New(), Now).IsSuccess.Should().BeTrue();
        x.Reopen("short", UserId.New(), Now).IsFailure.Should().BeTrue();
        x.Reopen("Closure was recorded against the wrong enrollment", UserId.New(), Now)
            .IsSuccess.Should()
            .BeTrue();
        x.Status.Should().Be(EnrollmentClosureStatus.Reopened);
    }

    private static EnrollmentClosureCheckSeed[] Checks() =>
        Enum.GetValues<EnrollmentClosureCheckType>()
            .Select(x => new EnrollmentClosureCheckSeed(
                x,
                EnrollmentClosureCheckStatus.Resolved,
                "Verified"
            ))
            .ToArray();

    private static DriveOS.SharedKernel.Results.Result<EnrollmentClosureCase> Create(
        IReadOnlyList<EnrollmentClosureCheckSeed> checks
    ) =>
        EnrollmentClosureCase.Create(
            OrganizationId.New(),
            PersonId.New(),
            DraftEnrollmentId.New(),
            EnrollmentStatus.Active,
            EnrollmentClosureReason.TrainingCompleted,
            DateOnly.FromDateTime(Now.UtcDateTime),
            "",
            UserId.New(),
            Now,
            checks
        );
}
