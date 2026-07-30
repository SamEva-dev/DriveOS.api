using DriveOS.Modules.Organizations.Domain.Branches;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules
    .Organizations.Branches;

public sealed class BranchManagerAssignmentTests
{
    [Fact]
    public void AssignPrimaryManager_ShouldCreateActiveAssignment()
    {
        Branch branch =
            CreateBranch();

        var managerUserId =
            UserId.New();

        var assignedByUserId =
            UserId.New();

        DateTimeOffset now =
            DateTimeOffset.UtcNow;

        var result =
            branch.AssignPrimaryManager(
                managerUserId,
                now,
                assignedByUserId,
                now);

        result.IsSuccess
            .Should()
            .BeTrue();

        branch.ManagerAssignments
            .Should()
            .ContainSingle();

        BranchManagerAssignment
            assignment =
                branch.ManagerAssignments
                    .Single();

        assignment.ManagerUserId
            .Should()
            .Be(managerUserId);

        assignment.Status
            .Should()
            .Be(
                BranchManagerAssignmentStatus
                    .Active);

        branch.HasActiveManagerAt(now)
            .Should()
            .BeTrue();
    }

    [Fact]
    public void AssignPrimaryManager_ShouldEndPreviousAssignment()
    {
        Branch branch =
            CreateBranch();

        var firstManager =
            UserId.New();

        var secondManager =
            UserId.New();

        var assignedBy =
            UserId.New();

        DateTimeOffset firstDate =
            DateTimeOffset.UtcNow;

        DateTimeOffset secondDate =
            firstDate.AddDays(10);

        branch.AssignPrimaryManager(
            firstManager,
            firstDate,
            assignedBy,
            firstDate);

        var result =
            branch.AssignPrimaryManager(
                secondManager,
                secondDate,
                assignedBy,
                secondDate);

        result.IsSuccess
            .Should()
            .BeTrue();

        branch.ManagerAssignments
            .Should()
            .HaveCount(2);

        BranchManagerAssignment
            oldAssignment =
                branch.ManagerAssignments
                    .Single(
                        assignment =>
                            assignment
                                .ManagerUserId ==
                            firstManager);

        oldAssignment.Status
            .Should()
            .Be(
                BranchManagerAssignmentStatus
                    .Ended);

        oldAssignment.EffectiveToUtc
            .Should()
            .Be(secondDate);

        branch
            .GetActiveManagerAssignmentAt(
                secondDate)
            ?.ManagerUserId
            .Should()
            .Be(secondManager);
    }

    [Fact]
    public void AssigningSameManager_ShouldBeIdempotent()
    {
        Branch branch =
            CreateBranch();

        var managerUserId =
            UserId.New();

        var assignedBy =
            UserId.New();

        DateTimeOffset now =
            DateTimeOffset.UtcNow;

        branch.AssignPrimaryManager(
            managerUserId,
            now,
            assignedBy,
            now);

        var result =
            branch.AssignPrimaryManager(
                managerUserId,
                now.AddDays(1),
                assignedBy,
                now.AddDays(1));

        result.IsSuccess
            .Should()
            .BeTrue();

        branch.ManagerAssignments
            .Should()
            .ContainSingle();
    }

    [Fact]
    public void DraftBranchWithoutManager_ShouldNotActivate()
    {
        Branch branch =
            CreateBranch();

        Action action = () =>
            branch.Activate(
                BranchStatusChangeReason.Create(
                    "Agence prête."),
                Guid.NewGuid(),
                DateTimeOffset.UtcNow);

        action.Should()
            .Throw<
                InvalidOperationException>();
    }

    [Fact]
    public void DraftBranchWithManager_ShouldActivate()
    {
        Branch branch =
            CreateBranch();

        DateTimeOffset now =
            DateTimeOffset.UtcNow;

        branch.AssignPrimaryManager(
            UserId.New(),
            now,
            UserId.New(),
            now);

        branch.Activate(
            BranchStatusChangeReason.Create(
                "Agence prête."),
            Guid.NewGuid(),
            now);

        branch.Status
            .Should()
            .Be(BranchStatus.Active);
    }

    private static Branch CreateBranch()
    {
        Result<BranchName> name =
            BranchName.Create(
                "Nice Centre");

        Result<BranchCode> code =
            BranchCode.Create(
                "NICE-CENTRE");

        Result<BranchAddress> address =
            BranchAddress.Create(
                "10 rue de France",
                null,
                "06000",
                "Nice",
                "FR");

        Result<Branch> branch =
            Branch.Create(
                BranchId.New(),
                OrganizationId.New(),
                name.Value,
                code.Value,
                BranchType
                    .DrivingSchoolAgency,
                address.Value,
                "Europe/Paris",
                false);

        return branch.Value;
    }
}