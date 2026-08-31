using DriveOS.Modules.Contracts.Domain.TrainingContracts;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using FluentAssertions;
using Xunit;

namespace DriveOS.UnitTests.Modules.Contracts.TrainingContracts;

public sealed class TrainingContractTerminationTests
{
    [Fact]
    public void Terminate_should_move_active_contract_to_terminated_and_keep_audit_data()
    {
        DateTimeOffset now = new(2026, 8, 17, 12, 0, 0, TimeSpan.Zero);
        TrainingContract contract = CreateActiveContract(now);
        UserId actor = new(Guid.NewGuid());

        Result result = contract.Terminate(
            "Résiliation demandée par l'élève après régularisation du dossier.",
            DateOnly.FromDateTime(now.UtcDateTime),
            actor,
            now);

        result.IsSuccess.Should().BeTrue();
        contract.Status.Should().Be(TrainingContractStatus.Terminated);
        contract.TerminatedByUserId.Should().Be(actor);
        contract.TerminatedAtUtc.Should().Be(now);
    }

    [Fact]
    public void Terminate_should_be_rejected_for_a_draft_contract()
    {
        TrainingContract contract = CreateDraft();
        DateTimeOffset now = new(2026, 8, 17, 12, 0, 0, TimeSpan.Zero);

        Result result = contract.Terminate(
            "Résiliation impossible car le contrat n'est pas actif.",
            DateOnly.FromDateTime(now.UtcDateTime),
            new UserId(Guid.NewGuid()),
            now);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TrainingContractErrors.TerminationNotAllowed);
    }

    private static TrainingContract CreateActiveContract(DateTimeOffset now)
    {
        TrainingContract contract = CreateDraft();
        UserId actor = new(Guid.NewGuid());
        contract.MarkGenerated("contracts/test.html", "test.html", "text/html", new string('A', 64), actor, now).IsSuccess.Should().BeTrue();
        var signatory = contract.AddSignatory(TrainingContractSignatoryKind.Student, new PersonId(Guid.NewGuid()), null, "Student", 1, true, null).Value;
        contract.DecideSignatoryAuthority(signatory.Id, true, null, actor, now).IsSuccess.Should().BeTrue();
        contract.MarkSentForSignature(new SignatureProcessId(Guid.NewGuid()), actor, now).IsSuccess.Should().BeTrue();
        contract.RecordSignatorySignature(signatory.Id, new SignatureEvidenceId(Guid.NewGuid()), actor, now).IsSuccess.Should().BeTrue();
        contract.Activate(actor, now).IsSuccess.Should().BeTrue();
        return contract;
    }

    private static TrainingContract CreateDraft()
    {
        var terms = TrainingContractTermsSnapshot.Create("B", 20m, "services", "payment", "cancellation", "booking", "student", "provider", "exam", "data").Value;
        PersonId student = new(Guid.NewGuid());
        OrganizationId organization = new(Guid.NewGuid());
        var parties = new[]
        {
            TrainingContractParty.ForPerson(TrainingContractPartyKind.Student, student, "Student").Value,
            TrainingContractParty.ForOrganization(TrainingContractPartyKind.TrainingProvider, organization, "School").Value,
        };
        return TrainingContract.CreateDraft(
            new TrainingContractId(Guid.NewGuid()), organization, new BranchId(Guid.NewGuid()), student,
            new CommercialOfferId(Guid.NewGuid()), 1, "CTR-001", new DateOnly(2026, 8, 1), new DateOnly(2027, 8, 1),
            1200m, "EUR", terms, parties).Value;
    }
}
