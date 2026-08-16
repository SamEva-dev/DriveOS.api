using DriveOS.Modules.Students.Domain.Suspensions;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.Students;

public sealed class EnrollmentReactivationTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 15, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Immediate_reactivation_rejects_failed_check()
    {
        var checks = Checks();
        checks[2] = new(
            ReactivationCheckType.Documents,
            ReactivationCheckStatus.Failed,
            "Expired document"
        );
        var result = Create(EnrollmentReactivationMode.Immediate, checks);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EnrollmentSuspensionErrors.ReactivationChecksIncomplete);
    }

    [Fact]
    public void Reactivation_requires_every_check_once()
    {
        var result = Create(EnrollmentReactivationMode.Immediate, Checks().Skip(1).ToArray());
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Conditional_reactivation_can_be_reviewed_before_apply()
    {
        var checks = Checks();
        checks[3] = new(
            ReactivationCheckType.Funding,
            ReactivationCheckStatus.Failed,
            "Funding pending"
        );
        var result = Create(
            EnrollmentReactivationMode.Conditional,
            checks,
            "Funding approval required"
        );
        result.IsSuccess.Should().BeTrue();
        var plan = result.Value;
        plan.Status.Should().Be(EnrollmentReactivationStatus.PendingConditions);
        plan.ReviewCheck(
                ReactivationCheckType.Funding,
                ReactivationCheckStatus.Valid,
                "Funding confirmed"
            )
            .IsSuccess.Should()
            .BeTrue();
        plan.Apply(DateOnly.FromDateTime(Now.UtcDateTime), Now).IsSuccess.Should().BeTrue();
        plan.Status.Should().Be(EnrollmentReactivationStatus.Applied);
    }

    [Fact]
    public void New_enrollment_option_never_reactivates_old_enrollment()
    {
        var plan = Create(EnrollmentReactivationMode.NewEnrollment, Checks()).Value;
        plan.Status.Should().Be(EnrollmentReactivationStatus.NewEnrollmentRequired);
        plan.Apply(DateOnly.FromDateTime(Now.UtcDateTime), Now).IsFailure.Should().BeTrue();
    }

    private static EnrollmentReactivationCheckSeed[] Checks() =>
        Enum.GetValues<ReactivationCheckType>()
            .Select(x => new EnrollmentReactivationCheckSeed(
                x,
                ReactivationCheckStatus.Valid,
                "Verified"
            ))
            .ToArray();

    private static DriveOS.SharedKernel.Results.Result<EnrollmentReactivation> Create(
        EnrollmentReactivationMode mode,
        IReadOnlyList<EnrollmentReactivationCheckSeed> checks,
        string conditions = ""
    ) =>
        EnrollmentReactivation.Create(
            OrganizationId.New(),
            PersonId.New(),
            DraftEnrollmentId.New(),
            EnrollmentSuspensionId.New(),
            mode,
            DateOnly.FromDateTime(Now.UtcDateTime),
            conditions,
            false,
            UserId.New(),
            Now,
            checks
        );
}
