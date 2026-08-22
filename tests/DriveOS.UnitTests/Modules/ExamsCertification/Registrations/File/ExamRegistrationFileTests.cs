using DriveOS.Modules.ExamsCertification.Domain.Registrations.File;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;
using Xunit;

namespace DriveOS.UnitTests.Modules.ExamsCertification.Registrations.File;

public sealed class ExamRegistrationFileTests
{
    [Fact]
    public void Refresh_ShouldBeReady_WhenAllRequiredRequirementsAreCompliant()
    {
        var file = ExamRegistrationFile.Create(
            new OrganizationId(Guid.NewGuid()), new ExamRegistrationId(Guid.NewGuid()), new PersonId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()), DateTimeOffset.UtcNow).Value;

        var items = new[]
        {
            new ExamRegistrationChecklistSnapshotItem("IdentityVerified", true, ExamRegistrationRequirementStatus.Compliant, "ok"),
            new ExamRegistrationChecklistSnapshotItem("OfficialDocument", true, ExamRegistrationRequirementStatus.Compliant, "ok"),
            new ExamRegistrationChecklistSnapshotItem("RegulatoryTrainingRecord", false, ExamRegistrationRequirementStatus.NotApplicable, "na")
        };

        var result = file.Refresh(items, "NEPH-123", null, new UserId(Guid.NewGuid()), DateTimeOffset.UtcNow);

        result.IsSuccess.Should().BeTrue();
        file.Status.Should().Be(ExamRegistrationFileStatus.Ready);
        file.CurrentVersion.Should().Be(1);
    }

    [Fact]
    public void Refresh_ShouldPreservePreviousRevision_WhenDossierChanges()
    {
        var actor = new UserId(Guid.NewGuid());
        var file = ExamRegistrationFile.Create(
            new OrganizationId(Guid.NewGuid()), new ExamRegistrationId(Guid.NewGuid()), new PersonId(Guid.NewGuid()),
            actor, DateTimeOffset.UtcNow).Value;

        file.Refresh(
            [new ExamRegistrationChecklistSnapshotItem("CandidateReference", true, ExamRegistrationRequirementStatus.Missing, "missing")],
            null, null, actor, DateTimeOffset.UtcNow);

        file.Refresh(
            [new ExamRegistrationChecklistSnapshotItem("CandidateReference", true, ExamRegistrationRequirementStatus.Compliant, "ok")],
            "NEPH-123", null, actor, DateTimeOffset.UtcNow.AddMinutes(1));

        file.Revisions.Should().HaveCount(2);
        file.Revisions.Select(x => x.Version).Should().BeEquivalentTo([1, 2]);
        file.Revisions.Single(x => x.Version == 1).CandidateReference.Should().BeNull();
        file.Revisions.Single(x => x.Version == 2).CandidateReference.Should().Be("NEPH-123");
    }
}
