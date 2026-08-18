using DriveOS.Modules.Contracts.Domain.SignatureProcesses;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.Contracts.TrainingContracts;

public sealed class SignatureProcessTests
{
    private const string DocumentHash = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";

    [Fact]
    public void RecordSignature_FirstRequiredSigner_MarksProcessPartiallySignedAndStoresEvidence()
    {
        TrainingContractSignatoryId first = TrainingContractSignatoryId.New();
        TrainingContractSignatoryId second = TrainingContractSignatoryId.New();
        SignatureProcess process = CreateProcess(first, second);
        DateTimeOffset signedAt = new(2026, 8, 17, 12, 0, 0, TimeSpan.Zero);

        var result = process.RecordSignature(
            first, DocumentHash, "Electronic", "Otp", "DriveOS", "sig-001", null,
            "127.0.0.1", "unit-test", signedAt, signedAt.AddSeconds(2), UserId.New());

        result.IsSuccess.Should().BeTrue();
        process.Status.Should().Be(SignatureProcessStatus.PartiallySigned);
        process.Evidence.Should().ContainSingle();
        process.Evidence.Single().DocumentSha256.Should().Be(DocumentHash);
    }

    [Fact]
    public void RecordSignature_OutOfSequentialOrder_Fails()
    {
        TrainingContractSignatoryId first = TrainingContractSignatoryId.New();
        TrainingContractSignatoryId second = TrainingContractSignatoryId.New();
        SignatureProcess process = CreateProcess(first, second);
        DateTimeOffset now = DateTimeOffset.UtcNow;

        var result = process.RecordSignature(
            second, DocumentHash, "Electronic", "Otp", "DriveOS", "sig-002", null,
            null, null, now, now.AddSeconds(1), UserId.New());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SignatureProcessErrors.SignatureOrderViolation);
        process.Evidence.Should().BeEmpty();
    }

    [Fact]
    public void RecordSignature_AllRequiredSigners_MarksProcessCompleted()
    {
        TrainingContractSignatoryId first = TrainingContractSignatoryId.New();
        TrainingContractSignatoryId second = TrainingContractSignatoryId.New();
        SignatureProcess process = CreateProcess(first, second);
        DateTimeOffset now = DateTimeOffset.UtcNow;

        process.RecordSignature(first, DocumentHash, "Electronic", "Otp", "DriveOS", "sig-003", null, null, null, now, now.AddSeconds(1), UserId.New()).IsSuccess.Should().BeTrue();
        process.RecordSignature(second, DocumentHash, "Electronic", "Otp", "DriveOS", "sig-004", "certificate-004", null, null, now.AddMinutes(1), now.AddMinutes(1).AddSeconds(1), UserId.New()).IsSuccess.Should().BeTrue();

        process.Status.Should().Be(SignatureProcessStatus.Completed);
        process.CompletedAtUtc.Should().NotBeNull();
        process.Evidence.Should().HaveCount(2);
    }

    [Fact]
    public void RecordSignature_WithDifferentDocumentHash_Fails()
    {
        TrainingContractSignatoryId first = TrainingContractSignatoryId.New();
        SignatureProcess process = CreateProcess(first, TrainingContractSignatoryId.New());
        DateTimeOffset now = DateTimeOffset.UtcNow;

        var result = process.RecordSignature(
            first,
            "BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB",
            "Electronic", "Otp", "DriveOS", "sig-005", null, null, null,
            now, now.AddSeconds(1), UserId.New());

        result.Error.Should().Be(SignatureProcessErrors.DocumentHashMismatch);
    }

    private static SignatureProcess CreateProcess(
        TrainingContractSignatoryId first,
        TrainingContractSignatoryId second)
    {
        var recipients = new[]
        {
            new SignatureProcessRecipientSnapshot(first, "Student", PersonId.New(), null, "Student", 1, true),
            new SignatureProcessRecipientSnapshot(second, "ProviderRepresentative", PersonId.New(), OrganizationId.New(), "Provider", 2, true),
        };

        return SignatureProcess.Create(
            SignatureProcessId.New(), OrganizationId.New(), TrainingContractId.New(), 1,
            "contracts/document.enc", DocumentHash, recipients, UserId.New(), DateTimeOffset.UtcNow).Value;
    }
}
