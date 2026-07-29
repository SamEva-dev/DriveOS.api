using DriveOS.Modules.Organizations.Domain.Organizations;
using FluentAssertions;

namespace DriveOS.UnitTests.Organizations;

public class OrganizationLifecycleTests
{
    [Fact]
    public void Activate_ShouldChangeStatus_WhenPendingActivation()
    {
        // Arrange
        Organization organization =
            OrganizationTestData.CreatePendingActivation();

        DateTimeOffset changedAtUtc =
            new(2026, 7, 28, 10, 0, 0, TimeSpan.Zero);

        Guid changedByUserId = Guid.NewGuid();

        // Act
        organization.Activate(
            OrganizationStatusChangeReason.Create(
                "Administrative checks completed."),
            changedByUserId,
            changedAtUtc);

        // Assert
        organization.Status.Should()
            .Be(OrganizationStatus.Active);

        organization.StatusHistory.Should()
            .ContainSingle();

        OrganizationStatusHistoryEntry entry =
            organization.StatusHistory.Single();

        entry.PreviousStatus.Should()
            .Be(OrganizationStatus.PendingActivation);

        entry.NewStatus.Should()
            .Be(OrganizationStatus.Active);

        entry.ChangedByUserId.Should()
            .Be(changedByUserId);

        entry.ChangedAtUtc.Should()
            .Be(changedAtUtc);
    }

    [Fact]
    public void Suspend_ShouldThrow_WhenOrganizationIsDraft()
    {
        Organization organization =
            OrganizationTestData.CreateDraft();

        Action action = () =>
            organization.Suspend(
                OrganizationStatusChangeReason.Create(
                    "Compliance issue."),
                Guid.NewGuid(),
                DateTimeOffset.UtcNow);

        action.Should()
            .Throw<InvalidOperationException>();

        organization.Status.Should()
            .Be(OrganizationStatus.Draft);

        organization.StatusHistory.Should()
            .BeEmpty();
    }

}
