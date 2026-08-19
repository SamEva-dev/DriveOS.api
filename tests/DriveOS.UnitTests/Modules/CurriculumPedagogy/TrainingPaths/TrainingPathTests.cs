using DriveOS.Modules.CurriculumPedagogy.Domain.TrainingPaths;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.CurriculumPedagogy.TrainingPaths;

public sealed class TrainingPathTests
{
    private static TrainingPath CreatePath() => TrainingPath.Create(
        TrainingPathId.New(),
        new OrganizationId(Guid.NewGuid()),
        new PersonId(Guid.NewGuid()),
        CurriculumVersionId.New(),
        TrainingMode.Standard,
        new DateOnly(2026, 9, 1),
        new DateOnly(2027, 2, 28),
        30m).Value;

    [Fact]
    public void Create_KeepsExactCurriculumVersionReference()
    {
        CurriculumVersionId versionId = CurriculumVersionId.New();
        TrainingPath path = TrainingPath.Create(
            TrainingPathId.New(),
            new OrganizationId(Guid.NewGuid()),
            new PersonId(Guid.NewGuid()),
            versionId,
            TrainingMode.Standard,
            new DateOnly(2026, 9, 1),
            null,
            20m).Value;

        path.CurriculumVersionId.Should().Be(versionId);
        path.Status.Should().Be(TrainingPathStatus.Draft);
    }

    [Fact]
    public void Activate_RequiresReadyState()
    {
        TrainingPath path = CreatePath();
        UserId actor = new(Guid.NewGuid());

        path.Activate(actor, DateTimeOffset.UtcNow).Error.Should().Be(TrainingPathErrors.ActivationNotAllowed);
        path.MarkReadyForActivation().IsSuccess.Should().BeTrue();
        path.Activate(actor, DateTimeOffset.UtcNow).IsSuccess.Should().BeTrue();
        path.Status.Should().Be(TrainingPathStatus.Active);
    }

    [Fact]
    public void AddMilestone_RejectsDuplicateCodeAndOrder()
    {
        TrainingPath path = CreatePath();
        path.AddMilestone(TrainingPathMilestoneId.New(), "M01", "Première étape", null, 1, null).IsSuccess.Should().BeTrue();

        path.AddMilestone(TrainingPathMilestoneId.New(), "m01", "Autre étape", null, 2, null)
            .Error.Should().Be(TrainingPathErrors.MilestoneCodeAlreadyExists);
        path.AddMilestone(TrainingPathMilestoneId.New(), "M02", "Autre étape", null, 1, null)
            .Error.Should().Be(TrainingPathErrors.MilestoneOrderAlreadyExists);
    }

    [Fact]
    public void Complete_RequiresAllMilestonesClosed()
    {
        TrainingPath path = CreatePath();
        UserId actor = new(Guid.NewGuid());
        TrainingPathMilestone milestone = path.AddMilestone(
            TrainingPathMilestoneId.New(), "ASSESSMENT", "Bilan intermédiaire", null, 1, null).Value;
        path.MarkReadyForActivation();
        path.Activate(actor, DateTimeOffset.UtcNow);

        path.Complete(DateTimeOffset.UtcNow).Error.Should().Be(TrainingPathErrors.OpenMilestonesRemain);
        path.CompleteMilestone(milestone.Id, actor, DateTimeOffset.UtcNow).IsSuccess.Should().BeTrue();
        path.Complete(DateTimeOffset.UtcNow).IsSuccess.Should().BeTrue();
        path.Status.Should().Be(TrainingPathStatus.Completed);
    }

    [Fact]
    public void SuspendedPath_CanBeReactivatedWithoutLosingCurriculumVersion()
    {
        TrainingPath path = CreatePath();
        CurriculumVersionId version = path.CurriculumVersionId;
        UserId actor = new(Guid.NewGuid());
        path.MarkReadyForActivation();
        path.Activate(actor, DateTimeOffset.UtcNow);

        path.Suspend("Interruption temporaire", DateTimeOffset.UtcNow).IsSuccess.Should().BeTrue();
        path.Status.Should().Be(TrainingPathStatus.Suspended);
        path.Reactivate().IsSuccess.Should().BeTrue();
        path.Status.Should().Be(TrainingPathStatus.Active);
        path.CurriculumVersionId.Should().Be(version);
    }
    [Fact]
    public void CancelMilestone_ClosesMilestoneAndAllowsPathCompletion()
    {
        TrainingPath path = CreatePath();
        UserId actor = new(Guid.NewGuid());
        TrainingPathMilestone milestone = path.AddMilestone(
            TrainingPathMilestoneId.New(), "OPTIONAL", "Jalon facultatif", null, 1, null).Value;
        path.MarkReadyForActivation();
        path.Activate(actor, DateTimeOffset.UtcNow);

        path.CancelMilestone(milestone.Id).IsSuccess.Should().BeTrue();
        path.Complete(DateTimeOffset.UtcNow).IsSuccess.Should().BeTrue();
        path.Status.Should().Be(TrainingPathStatus.Completed);
    }

    [Fact]
    public void CancelledPath_CannotBeReactivatedOrCompleted()
    {
        TrainingPath path = CreatePath();
        path.Cancel("Changement de projet", DateTimeOffset.UtcNow).IsSuccess.Should().BeTrue();

        path.Reactivate().Error.Should().Be(TrainingPathErrors.ReactivationNotAllowed);
        path.Complete(DateTimeOffset.UtcNow).Error.Should().Be(TrainingPathErrors.CompletionNotAllowed);
        path.Status.Should().Be(TrainingPathStatus.Cancelled);
    }

    [Fact]
    public void MilestoneLifecycle_RequiresActiveTrainingPathToStart()
    {
        TrainingPath path = CreatePath();
        TrainingPathMilestone milestone = path.AddMilestone(
            TrainingPathMilestoneId.New(), "M01", "Premier jalon", null, 1, null).Value;

        path.StartMilestone(milestone.Id).Error.Should().Be(TrainingPathErrors.ModificationNotAllowed);

        UserId actor = new(Guid.NewGuid());
        path.MarkReadyForActivation();
        path.Activate(actor, DateTimeOffset.UtcNow);
        path.StartMilestone(milestone.Id).IsSuccess.Should().BeTrue();
        path.CompleteMilestone(milestone.Id, actor, DateTimeOffset.UtcNow).IsSuccess.Should().BeTrue();
    }

}
