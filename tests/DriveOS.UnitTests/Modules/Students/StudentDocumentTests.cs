using DriveOS.Modules.Students.Domain.Documents;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.Students;

public sealed class StudentDocumentTests
{
    [Fact]
    public void UploadingReplacement_ShouldPreservePreviousVersion()
    {
        var d = Create();
        var actor = UserId.New();
        d.AddVersion(
            "identity.pdf",
            "application/pdf",
            100,
            "a",
            "private/v1",
            actor,
            DateTimeOffset.UtcNow
        );
        d.AddVersion(
            "identity-v2.pdf",
            "application/pdf",
            110,
            "b",
            "private/v2",
            actor,
            DateTimeOffset.UtcNow
        );
        d.CurrentVersion.Should().Be(2);
        d.Versions.Should().HaveCount(2);
        d.Versions.Single(x => x.VersionNumber == 1).IsCurrent.Should().BeFalse();
        d.Versions.Single(x => x.VersionNumber == 2).IsCurrent.Should().BeTrue();
    }

    [Fact]
    public void Rejection_ShouldRequireAReason()
    {
        var d = Create();
        var actor = UserId.New();
        d.AddVersion(
            "identity.pdf",
            "application/pdf",
            100,
            "a",
            "private/v1",
            actor,
            DateTimeOffset.UtcNow
        );
        d.Validate(false, "", actor, DateTimeOffset.UtcNow)
            .Error.Should()
            .Be(StudentDocumentErrors.ReasonRequired);
    }

    [Fact]
    public void Download_ShouldBeAudited()
    {
        var d = Create();
        var actor = UserId.New();
        Guid version = d.AddVersion(
            "identity.pdf",
            "application/pdf",
            100,
            "a",
            "private/v1",
            actor,
            DateTimeOffset.UtcNow
        ).Value;
        d.LogDownload(version, actor, DateTimeOffset.UtcNow);
        d.AccessLogs.Single().Action.Should().Be(StudentDocumentAccessAction.Downloaded);
    }

    private static StudentDocument Create() =>
        StudentDocument
            .Request(
                OrganizationId.New(),
                PersonId.New(),
                null,
                "IdentityProof",
                StudentDocumentCategory.Identity,
                StudentDocumentVisibility.AdministrativeStaff,
                null,
                UserId.New(),
                DateTimeOffset.UtcNow
            )
            .Value;
}
