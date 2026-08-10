using DriveOS.Modules.Organizations.Domain.Branches;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.Organizations.Branches;

public sealed class BranchLifecycleTests
{
    private static readonly BranchId BranchId =
        new(
            Guid.Parse(
                "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));

    private static readonly OrganizationId
        OrganizationId =
            new(
                Guid.Parse(
                    "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));

    private static readonly Guid ChangedByUserId =
        Guid.Parse(
            "cccccccc-cccc-cccc-cccc-cccccccccccc");

    private static readonly DateTimeOffset ChangedAtUtc =
        new(
            2026,
            7,
            29,
            12,
            0,
            0,
            TimeSpan.Zero);

    [Fact]
    public void Activate_WhenDraft_ShouldSucceed()
    {
        Branch branch =
            CreateDraftBranch();

        branch.AssignPrimaryManager(
            UserId.New(),
            ChangedAtUtc,
            UserId.New(),
            ChangedAtUtc);

        branch.Activate(
            BranchStatusChangeReason.Create(
                "Agence prête à démarrer."),
            ChangedByUserId,
            ChangedAtUtc);

        branch.Status.Should()
            .Be(BranchStatus.Active);

        BranchStatusHistoryEntry entry =
            branch.StatusHistory
                .Should()
                .ContainSingle()
                .Subject;

        entry.PreviousStatus.Should()
            .Be(BranchStatus.Draft);

        entry.NewStatus.Should()
            .Be(BranchStatus.Active);

        entry.Reason.Value.Should()
            .Be(
                "Agence prête à démarrer.");

        entry.ChangedByUserId.Should()
            .Be(ChangedByUserId);

        entry.ChangedAtUtc.Should()
            .Be(ChangedAtUtc);
    }

    [Fact]
    public void Restrict_WhenActive_ShouldSucceed()
    {
        Branch branch =
            CreateActiveBranch();

        branch.Restrict(
            BranchStatusChangeReason.Create(
                "Activité temporairement limitée."),
            ChangedByUserId,
            ChangedAtUtc);

        branch.Status.Should()
            .Be(BranchStatus.Restricted);
    }

    [Fact]
    public void Suspend_WhenRestricted_ShouldSucceed()
    {
        Branch branch =
            CreateRestrictedBranch();

        branch.Suspend(
            BranchStatusChangeReason.Create(
                "Non-conformité critique."),
            ChangedByUserId,
            ChangedAtUtc);

        branch.Status.Should()
            .Be(BranchStatus.Suspended);
    }

    [Fact]
    public void Reactivate_WhenSuspended_ShouldSucceed()
    {
        Branch branch =
            CreateSuspendedBranch();

        branch.Reactivate(
            BranchStatusChangeReason.Create(
                "Conformité rétablie."),
            ChangedByUserId,
            ChangedAtUtc);

        branch.Status.Should()
            .Be(BranchStatus.Active);
    }

    [Fact]
    public void Close_WhenPrimary_ShouldRemovePrimaryDesignation()
    {
        Branch branch =
            CreateActiveBranch(
                isPrimary: true);

        branch.Close(
            BranchStatusChangeReason.Create(
                "Fermeture définitive."),
            ChangedByUserId,
            ChangedAtUtc);

        branch.Status.Should()
            .Be(BranchStatus.Closed);

        branch.IsPrimary.Should()
            .BeFalse();
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldThrow()
    {
        Branch branch =
            CreateActiveBranch();

        Action action = () =>
            branch.Activate(
                BranchStatusChangeReason.Create(
                    "Nouvelle activation."),
                ChangedByUserId,
                ChangedAtUtc);

        action.Should()
            .Throw<
                InvalidOperationException>();
    }

    [Fact]
    public void Reactivate_WhenClosed_ShouldThrow()
    {
        Branch branch =
            CreateClosedBranch();

        Action action = () =>
            branch.Reactivate(
                BranchStatusChangeReason.Create(
                    "Tentative de réouverture."),
                ChangedByUserId,
                ChangedAtUtc);

        action.Should()
            .Throw<
                InvalidOperationException>();

        branch.Status.Should()
            .Be(BranchStatus.Closed);
    }

    private static Branch
        CreateDraftBranch(
            bool isPrimary = false)
    {
        Result<Branch> result =
            Branch.Create(
                BranchId,
                OrganizationId,
                BranchName.Create(
                    "Nice Centre").Value,
                BranchCode.Create(
                    "NICE-CENTRE").Value,
                BranchType.DrivingSchoolAgency,
                BranchAddress.Create(
                    "10 rue de France",
                    null,
                    "06000",
                    "Nice",
                    "FR").Value,
                "Europe/Paris",
                isPrimary);

        return result.Value;
    }

    private static Branch
        CreateActiveBranch(
            bool isPrimary = false)
    {
        Branch branch =
            CreateDraftBranch(
                isPrimary);

        Result managerAssignmentResult =
            branch.AssignPrimaryManager(
                UserId.New(),
                ChangedAtUtc,
                UserId.New(),
                ChangedAtUtc);

        managerAssignmentResult.IsSuccess.Should()
            .BeTrue();

        branch.Activate(
            BranchStatusChangeReason.Create(
                "Activation initiale."),
            ChangedByUserId,
            ChangedAtUtc);

        return branch;
    }

    private static Branch
        CreateRestrictedBranch()
    {
        Branch branch =
            CreateActiveBranch();

        branch.Restrict(
            BranchStatusChangeReason.Create(
                "Restriction initiale."),
            ChangedByUserId,
            ChangedAtUtc);

        return branch;
    }

    private static Branch
        CreateSuspendedBranch()
    {
        Branch branch =
            CreateActiveBranch();

        branch.Suspend(
            BranchStatusChangeReason.Create(
                "Suspension initiale."),
            ChangedByUserId,
            ChangedAtUtc);

        return branch;
    }

    private static Branch
        CreateClosedBranch()
    {
        Branch branch =
            CreateActiveBranch();

        branch.Close(
            BranchStatusChangeReason.Create(
                "Fermeture initiale."),
            ChangedByUserId,
            ChangedAtUtc);

        return branch;
    }
}
