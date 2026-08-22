using DriveOS.Modules.ExamsCertification.Domain.Registrations.Submissions;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;
using Xunit;

namespace DriveOS.UnitTests.Modules.ExamsCertification.Registrations.Submissions;

public sealed class ExamRegistrationSubmissionTests
{
    [Fact]
    public void Submission_ShouldPreserveExactFileRevisionAndPayload()
    {
        Guid operationId = Guid.NewGuid();
        Guid revisionId = Guid.NewGuid();
        string payload = "{\"schemaVersion\":\"driveos.exam-registration.v1\"}";

        var result = ExamRegistrationSubmission.Create(
            new OrganizationId(Guid.NewGuid()),
            ExamRegistrationId.New(),
            ExamRegistrationFileId.New(),
            revisionId,
            3,
            2,
            "manual",
            payload,
            operationId,
            "ABC",
            new UserId(Guid.NewGuid()),
            DateTimeOffset.UtcNow);

        result.IsSuccess.Should().BeTrue();
        result.Value.FileRevisionId.Should().Be(revisionId);
        result.Value.FileVersion.Should().Be(3);
        result.Value.SubmissionVersion.Should().Be(2);
        result.Value.PayloadJson.Should().Be(payload);
        result.Value.MatchesOperation(operationId, "ABC").Should().BeTrue();
    }

    [Fact]
    public void CorrectionRequested_ShouldKeepStableDriveOSErrorInsteadOfProviderMessage()
    {
        var actor = new UserId(Guid.NewGuid());
        var submission = ExamRegistrationSubmission.Create(
            new OrganizationId(Guid.NewGuid()), ExamRegistrationId.New(), ExamRegistrationFileId.New(), Guid.NewGuid(),
            1, 1, "rdvpermis", "{}", Guid.NewGuid(), "ABC", actor, DateTimeOffset.UtcNow).Value;

        var result = submission.MarkCorrectionRequested(
            "Exams.RegistrationSubmission.CorrectionRequested",
            "errors.exams.registrationSubmission.correctionRequested",
            "REMOTE_42",
            "{\"message\":\"provider-specific text\"}",
            actor,
            DateTimeOffset.UtcNow);

        result.IsSuccess.Should().BeTrue();
        submission.Status.Should().Be(ExamRegistrationSubmissionStatus.CorrectionRequested);
        submission.ErrorCode.Should().Be("Exams.RegistrationSubmission.CorrectionRequested");
        submission.ErrorMessageKey.Should().Be("errors.exams.registrationSubmission.correctionRequested");
        submission.ProviderResponseCode.Should().Be("REMOTE_42");
    }
}
