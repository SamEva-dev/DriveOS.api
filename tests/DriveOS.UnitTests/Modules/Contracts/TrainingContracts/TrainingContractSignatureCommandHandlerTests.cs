using DriveOS.Modules.Contracts.Application.TrainingContracts.Signature;
using DriveOS.Modules.Contracts.Application.TrainingContracts.Signature.Record;
using DriveOS.Modules.Contracts.Domain.SignatureProcesses;
using DriveOS.Modules.Contracts.Domain.TrainingContracts;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;
using static DriveOS.UnitTests.Modules.Contracts.ContractsApplicationTestFixture;

namespace DriveOS.UnitTests.Modules.Contracts.TrainingContracts;

public sealed class TrainingContractSignatureCommandHandlerTests
{
    [Fact]
    public async Task SendForSignature_ShouldCreateProcessAndCommitOnce()
    {
        DateTimeOffset now = new(2026, 8, 17, 12, 0, 0, TimeSpan.Zero);
        OrganizationId organizationId = OrganizationId.New();
        TrainingContract contract = CreateDraft(
            organizationId,
            new DateOnly(2026, 8, 1),
            new DateOnly(2027, 8, 1));
        contract.AddSignatory(
            TrainingContractSignatoryKind.Student,
            contract.StudentId,
            null,
            "Student",
            1,
            true,
            null).IsSuccess.Should().BeTrue();
        contract.MarkGenerated(
            "contracts/test/document",
            "contract.html",
            "text/html",
            new string('A', 64),
            UserId.New(),
            now).IsSuccess.Should().BeTrue();

        var signatureRepository = new FakeSignatureProcessRepository();
        var uow = new FakeContractsUnitOfWork();
        var handler = new SendTrainingContractForSignatureCommandHandler(
            new FakeTrainingContractRepository(contract),
            signatureRepository,
            uow,
            new FakeClock(now));

        var result = await handler.Handle(
            new SendTrainingContractForSignatureCommand(
                organizationId,
                contract.Id,
                UserId.New()),
            default);

        result.IsSuccess.Should().BeTrue();
        contract.Status.Should().Be(TrainingContractStatus.SentForSignature);
        signatureRepository.Added.Should().NotBeNull();
        signatureRepository.Added!.ContractId.Should().Be(contract.Id);
        signatureRepository.Added.ContractVersionNumber.Should().Be(contract.CurrentVersionNumber);
        signatureRepository.Added.DocumentSha256.Should().Be(contract.GeneratedDocumentSha256);
        uow.CommitCount.Should().Be(1);
    }

    [Fact]
    public async Task SendForSignature_WhenProcessAlreadyExists_ShouldFailWithoutCommit()
    {
        DateTimeOffset now = new(2026, 8, 17, 12, 0, 0, TimeSpan.Zero);
        OrganizationId organizationId = OrganizationId.New();
        TrainingContract contract = CreateDraft(
            organizationId,
            new DateOnly(2026, 8, 1),
            null);
        contract.AddSignatory(
            TrainingContractSignatoryKind.Student,
            contract.StudentId,
            null,
            "Student",
            1,
            true,
            null).IsSuccess.Should().BeTrue();
        contract.MarkGenerated(
            "contracts/test/document",
            "contract.html",
            "text/html",
            new string('A', 64),
            UserId.New(),
            now).IsSuccess.Should().BeTrue();

        var signatureRepository = new FakeSignatureProcessRepository { ExistingForVersion = true };
        var uow = new FakeContractsUnitOfWork();
        var handler = new SendTrainingContractForSignatureCommandHandler(
            new FakeTrainingContractRepository(contract),
            signatureRepository,
            uow,
            new FakeClock(now));

        var result = await handler.Handle(
            new SendTrainingContractForSignatureCommand(
                organizationId,
                contract.Id,
                UserId.New()),
            default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TrainingContractErrors.SendForSignatureNotAllowed);
        signatureRepository.Added.Should().BeNull();
        uow.CommitCount.Should().Be(0);
    }

    [Fact]
    public async Task RecordSignature_ShouldSynchronizeProcessAndContractAndCommitOnce()
    {
        DateTimeOffset now = new(2026, 8, 17, 12, 0, 0, TimeSpan.Zero);
        OrganizationId organizationId = OrganizationId.New();
        TrainingContract contract = CreateDraft(
            organizationId,
            new DateOnly(2026, 8, 1),
            null);
        TrainingContractSignatory signatory = contract.AddSignatory(
            TrainingContractSignatoryKind.Student,
            contract.StudentId,
            null,
            "Student",
            1,
            true,
            null).Value;
        contract.MarkGenerated(
            "contracts/test/document",
            "contract.html",
            "text/html",
            new string('A', 64),
            UserId.New(),
            now).IsSuccess.Should().BeTrue();

        var recipients = new[]
        {
            new SignatureProcessRecipientSnapshot(
                signatory.Id,
                signatory.Kind.ToString(),
                signatory.PersonId,
                signatory.RepresentedOrganizationId,
                signatory.DisplayName,
                signatory.SigningOrder,
                signatory.IsRequired)
        };
        SignatureProcess process = SignatureProcess.Create(
            SignatureProcessId.New(),
            organizationId,
            contract.Id,
            contract.CurrentVersionNumber,
            contract.GeneratedDocumentReference!,
            contract.GeneratedDocumentSha256!,
            recipients,
            UserId.New(),
            now).Value;
        contract.MarkSentForSignature(process.Id, UserId.New(), now).IsSuccess.Should().BeTrue();

        var processRepository = new FakeSignatureProcessRepository();
        processRepository.Seed(process);
        var uow = new FakeContractsUnitOfWork();
        UserId actor = UserId.New();
        var handler = new RecordTrainingContractSignatureCommandHandler(
            new FakeTrainingContractRepository(contract),
            processRepository,
            uow,
            new FakeClock(now));

        var result = await handler.Handle(
            new RecordTrainingContractSignatureCommand(
                organizationId,
                contract.Id,
                process.Id,
                signatory.Id,
                contract.GeneratedDocumentSha256!,
                "Electronic",
                "OTP",
                "TestProvider",
                "provider-signature-001",
                "certificate-001",
                "127.0.0.1",
                "DriveOS tests",
                now,
                actor),
            default);

        result.IsSuccess.Should().BeTrue();
        process.Status.Should().Be(SignatureProcessStatus.Completed);
        process.Evidence.Should().ContainSingle();
        contract.Status.Should().Be(TrainingContractStatus.Signed);
        contract.LastModifiedByUserId.Should().Be(actor);
        uow.CommitCount.Should().Be(1);
    }

    [Fact]
    public async Task RecordSignature_FromAnotherTenant_ShouldReturnNotFoundWithoutCommit()
    {
        DateTimeOffset now = new(2026, 8, 17, 12, 0, 0, TimeSpan.Zero);
        OrganizationId ownerOrganizationId = OrganizationId.New();
        TrainingContract contract = CreateDraft(
            ownerOrganizationId,
            new DateOnly(2026, 8, 1),
            null);
        var uow = new FakeContractsUnitOfWork();
        var handler = new RecordTrainingContractSignatureCommandHandler(
            new FakeTrainingContractRepository(contract),
            new FakeSignatureProcessRepository(),
            uow,
            new FakeClock(now));

        var result = await handler.Handle(
            new RecordTrainingContractSignatureCommand(
                OrganizationId.New(),
                contract.Id,
                SignatureProcessId.New(),
                TrainingContractSignatoryId.New(),
                new string('A', 64),
                "Electronic",
                "OTP",
                "TestProvider",
                "provider-signature-002",
                null,
                null,
                null,
                now,
                UserId.New()),
            default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TrainingContractErrors.NotFound);
        uow.CommitCount.Should().Be(0);
    }
}
