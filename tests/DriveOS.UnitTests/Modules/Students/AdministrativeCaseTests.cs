using DriveOS.Modules.Students.Domain.Administration;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.Students;

public sealed class AdministrativeCaseTests
{
    [Fact]
    public void ValidatingAllRequirements_ShouldMakeTheCaseCompliant()
    {
        var item = AdministrativeCase.Create(OrganizationId.New(), PersonId.New()).Value;
        UserId actor = UserId.New();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        Guid requirement = item.UpsertRequirement(
            null,
            "IDENTITY",
            "students.requirements.identity",
            true,
            null,
            "FR-B",
            actor,
            now
        ).Value;
        item.DecideRequirement(
            requirement,
            AdministrativeRequirementStatus.Validated,
            "Document verified",
            actor,
            now
        );
        item.Status.Should().Be(AdministrativeStatus.Compliant);
        item.History.Should().HaveCount(2);
    }

    [Fact]
    public void ActiveBlock_ShouldRemainSeparateAndForceBlockedStatus()
    {
        var item = AdministrativeCase.Create(OrganizationId.New(), PersonId.New()).Value;
        UserId actor = UserId.New();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        Guid block = item.AddBlock(
            "MISSING_CONSENT",
            "Required consent is missing",
            actor,
            now
        ).Value;
        item.Status.Should().Be(AdministrativeStatus.Blocked);
        item.ReleaseBlock(block, "Consent received", actor, now);
        item.Status.Should().Be(AdministrativeStatus.ToComplete);
    }

    [Fact]
    public void ApprovedException_ShouldWaiveOnlyItsRequirement()
    {
        var item = AdministrativeCase.Create(OrganizationId.New(), PersonId.New()).Value;
        UserId actor = UserId.New();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        Guid requirement = item.UpsertRequirement(
            null,
            "PROOF",
            "students.requirements.proof",
            true,
            null,
            "FR-B",
            actor,
            now
        ).Value;
        Guid exceptionId = item.RequestException(
            requirement,
            "Legacy proof unavailable",
            actor,
            now
        ).Value;
        item.DecideException(
            exceptionId,
            true,
            "Manager approved documented exception",
            actor,
            now
        );
        item.Requirements.Single().Status.Should().Be(AdministrativeRequirementStatus.Waived);
        item.Status.Should().Be(AdministrativeStatus.Compliant);
    }
}
