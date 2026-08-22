using DriveOS.Modules.ExamsCertification.Domain.Certifications;
using DriveOS.SharedKernel.Identifiers;
using Xunit;

namespace DriveOS.UnitTests.Modules.ExamsCertification.Certifications;

public sealed class ExamAttestationTests
{
    [Fact]
    public void Issue_CreatesGeneratedAttestationWithInitialRevision()
    {
        var result = ExamAttestation.Issue(
            new OrganizationId(Guid.NewGuid()),
            new ExamResultId(Guid.NewGuid()),
            2,
            new ExamAttemptId(Guid.NewGuid()),
            new ExamRegistrationId(Guid.NewGuid()),
            new PersonId(Guid.NewGuid()),
            1,
            ExamAttestationType.SuccessAttestation,
            "EXM-2026-001",
            null,
            "exam-success",
            3,
            new DocumentId(Guid.NewGuid()),
            new string('a', 64),
            null,
            null,
            Guid.NewGuid(),
            "fp",
            new UserId(Guid.NewGuid()),
            DateTimeOffset.UtcNow);

        Assert.True(result.IsSuccess);
        Assert.Equal(ExamAttestationStatus.Generated, result.Value.Status);
        Assert.Equal(1, result.Value.CurrentVersion);
        Assert.Single(result.Value.Revisions);
        Assert.Equal(result.Value.CurrentVersion, result.Value.CurrentRevision.Version);
    }

    [Fact]
    public void Revoke_RequiresReason_AndKeepsCurrentRevisionDocumentReference()
    {
        var documentId = new DocumentId(Guid.NewGuid());
        var issued = ExamAttestation.Issue(
            new OrganizationId(Guid.NewGuid()),
            new ExamResultId(Guid.NewGuid()),
            1,
            new ExamAttemptId(Guid.NewGuid()),
            new ExamRegistrationId(Guid.NewGuid()),
            new PersonId(Guid.NewGuid()),
            2,
            ExamAttestationType.ResultStatement,
            "EXM-2026-002",
            null,
            "exam-result",
            1,
            documentId,
            new string('b', 64),
            null,
            null,
            Guid.NewGuid(),
            "fp",
            new UserId(Guid.NewGuid()),
            DateTimeOffset.UtcNow).Value;

        var revoked = issued.Revoke(
            "official-result-corrected",
            "Corrected by authority",
            new UserId(Guid.NewGuid()),
            DateTimeOffset.UtcNow);

        Assert.True(revoked.IsSuccess);
        Assert.Equal(ExamAttestationStatus.Revoked, issued.Status);
        Assert.Equal(documentId, issued.CurrentRevision.DocumentId);
    }

    [Fact]
    public void Supersede_DoesNotRewriteGeneratedDocumentRevision()
    {
        var documentId = new DocumentId(Guid.NewGuid());
        var issued = ExamAttestation.Issue(
            new OrganizationId(Guid.NewGuid()),
            new ExamResultId(Guid.NewGuid()),
            1,
            new ExamAttemptId(Guid.NewGuid()),
            new ExamRegistrationId(Guid.NewGuid()),
            new PersonId(Guid.NewGuid()),
            1,
            ExamAttestationType.PresentationAttestation,
            "EXM-2026-003",
            null,
            "exam-presentation",
            1,
            documentId,
            new string('c', 64),
            null,
            null,
            Guid.NewGuid(),
            "fp",
            new UserId(Guid.NewGuid()),
            DateTimeOffset.UtcNow).Value;

        issued.Supersede(DateTimeOffset.UtcNow);

        Assert.Equal(ExamAttestationStatus.Superseded, issued.Status);
        Assert.NotNull(issued.SupersededAtUtc);
        Assert.Equal(documentId, issued.CurrentRevision.DocumentId);
        Assert.Single(issued.Revisions);
    }
}
