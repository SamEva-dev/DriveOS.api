using DriveOS.Modules.Students.Domain.Checklists;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.Students;

public sealed class EnrollmentChecklistTests
{
    [Fact]
    public void BlockingItems_MustBeCompletedBeforeActivation()
    {
        var c = Create();
        var actor = UserId.New();
        Guid item = c.UpsertRule(
            Guid.NewGuid(),
            "IDENTITY",
            "students.checklist.identity",
            ChecklistCategory.Identity,
            true,
            "identity",
            null,
            null,
            actor,
            DateTimeOffset.UtcNow
        ).Value;
        c.CanActivate().Should().BeFalse();
        c.ChangeStatus(item, ChecklistItemStatus.Completed, null, actor, DateTimeOffset.UtcNow);
        c.CanActivate().Should().BeTrue();
    }

    [Fact]
    public void Waiver_RequiresAReason()
    {
        var c = Create();
        var actor = UserId.New();
        Guid item = c.UpsertRule(
            Guid.NewGuid(),
            "PROOF",
            "students.checklist.proof",
            ChecklistCategory.Documents,
            true,
            "documents",
            null,
            null,
            actor,
            DateTimeOffset.UtcNow
        ).Value;
        c.ChangeStatus(item, ChecklistItemStatus.Waived, null, actor, DateTimeOffset.UtcNow)
            .Error.Should()
            .Be(EnrollmentChecklistErrors.ReasonRequired);
    }

    [Fact]
    public void Item_KeepsResponsibleAndDeadline()
    {
        var c = Create();
        var actor = UserId.New();
        Guid responsible = Guid.NewGuid();
        DateTimeOffset due = DateTimeOffset.UtcNow.AddDays(5);
        Guid item = c.UpsertRule(
            Guid.NewGuid(),
            "CONTRACT",
            "students.checklist.contract",
            ChecklistCategory.Contract,
            true,
            "contracts",
            responsible,
            due,
            actor,
            DateTimeOffset.UtcNow
        ).Value;
        var value = c.Items.Single(x => x.Id == item);
        value.ResponsibleUserId.Should().Be(responsible);
        value.DueAtUtc.Should().Be(due);
    }

    private static EnrollmentChecklist Create() =>
        EnrollmentChecklist
            .Create(OrganizationId.New(), PersonId.New(), DraftEnrollmentId.New())
            .Value;
}
