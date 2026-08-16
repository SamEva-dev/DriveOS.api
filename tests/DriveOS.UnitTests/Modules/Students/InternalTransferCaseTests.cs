using DriveOS.Modules.Students.Domain.Transfers;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.Students;

public sealed class InternalTransferCaseTests
{
    [Fact]
    public void Create_ShouldRejectSameBranch()
    {
        var branch = BranchId.New();
        Create(branch, branch, InternalTransferMode.Immediate, null, null)
            .Error.Should()
            .Be(InternalTransferErrors.SameBranch);
    }

    [Fact]
    public void ScheduledTransfer_ShouldRequireExplicitDate()
    {
        Create(BranchId.New(), BranchId.New(), InternalTransferMode.EffectiveOnDate, null, null)
            .Error.Should()
            .Be(InternalTransferErrors.EffectiveDateRequired);
    }

    [Fact]
    public void Validate_ShouldRejectBlockingImpact()
    {
        var now = DateTimeOffset.UtcNow;
        var transfer = InternalTransferCase
            .Create(
                OrganizationId.New(),
                PersonId.New(),
                BranchId.New(),
                BranchId.New(),
                InternalTransferMode.Immediate,
                InternalTransferElement.Enrollment,
                null,
                null,
                "Move",
                [
                    new(
                        InternalTransferImpactType.Enrollment,
                        1,
                        InternalTransferImpactStatus.Blocked,
                        "blocked",
                        true
                    ),
                ],
                UserId.New(),
                now
            )
            .Value;
        transfer
            .Validate(UserId.New(), now.AddMinutes(1))
            .Error.Should()
            .Be(InternalTransferErrors.BlockingImpact);
    }

    [Fact]
    public void ImmediateValidation_ShouldApplyWithoutCreatingAnotherStudent()
    {
        var now = DateTimeOffset.UtcNow;
        var transfer = InternalTransferCase
            .Create(
                OrganizationId.New(),
                PersonId.New(),
                BranchId.New(),
                BranchId.New(),
                InternalTransferMode.Immediate,
                InternalTransferElement.All,
                null,
                null,
                "Move",
                [
                    new(
                        InternalTransferImpactType.Enrollment,
                        1,
                        InternalTransferImpactStatus.Passed,
                        "ok",
                        false
                    ),
                ],
                UserId.New(),
                now
            )
            .Value;
        transfer.Validate(UserId.New(), now.AddMinutes(1)).IsSuccess.Should().BeTrue();
        transfer.Status.Should().Be(InternalTransferStatus.Applied);
        transfer.StudentId.IsEmpty.Should().BeFalse();
    }

    private static DriveOS.SharedKernel.Results.Result<InternalTransferCase> Create(
        BranchId source,
        BranchId target,
        InternalTransferMode mode,
        DateOnly? date,
        DateOnly? until
    ) =>
        InternalTransferCase.Create(
            OrganizationId.New(),
            PersonId.New(),
            source,
            target,
            mode,
            InternalTransferElement.Enrollment,
            date,
            until,
            "Move",
            [],
            UserId.New(),
            DateTimeOffset.UtcNow
        );
}
