using DriveOS.Modules.Contracts.Domain.TrainingContracts;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.Contracts.TrainingContracts;

public sealed class TrainingContractSignatureTests
{
    [Fact]
    public void RecordingRequiredSignatures_TransitionsPartiallySignedThenSigned()
    {
        OrganizationId organizationId = OrganizationId.New();
        PersonId studentId = PersonId.New();
        UserId actor = UserId.New();
        DateTimeOffset now = DateTimeOffset.UtcNow;

        TrainingContract contract = TrainingContract.CreateDraft(
            TrainingContractId.New(), organizationId, BranchId.New(), studentId,
            CommercialOfferId.New(), 1, "CTR-SIGN-001", new DateOnly(2026, 9, 1), null,
            1500m, "EUR", CreateTerms(), CreateParties(organizationId, studentId)).Value;

        TrainingContractSignatory student = contract.AddSignatory(
            TrainingContractSignatoryKind.Student, studentId, null, "Student", 1, true, null).Value;
        TrainingContractSignatory provider = contract.AddSignatory(
            TrainingContractSignatoryKind.ProviderRepresentative, PersonId.New(), organizationId,
            "Provider", 2, true, "delegation-001").Value;

        contract.DecideSignatoryAuthority(provider.Id, true, null, actor, now).IsSuccess.Should().BeTrue();
        contract.MarkGenerated("doc/ref", "contract.html", "text/html", new string('A', 64), actor, now).IsSuccess.Should().BeTrue();
        contract.MarkSentForSignature(SignatureProcessId.New(), actor, now).IsSuccess.Should().BeTrue();

        contract.RecordSignatorySignature(student.Id, SignatureEvidenceId.New(), actor, now.AddMinutes(1)).IsSuccess.Should().BeTrue();
        contract.Status.Should().Be(TrainingContractStatus.PartiallySigned);

        contract.RecordSignatorySignature(provider.Id, SignatureEvidenceId.New(), actor, now.AddMinutes(2)).IsSuccess.Should().BeTrue();
        contract.Status.Should().Be(TrainingContractStatus.Signed);
        contract.Signatories.Should().OnlyContain(x => x.Status == TrainingContractSignatoryStatus.Signed);
    }

    private static TrainingContractTermsSnapshot CreateTerms() =>
        TrainingContractTermsSnapshot.Create(
            "B-MANUAL", 20m, "Driving lessons", "3 installments", "Cancellation terms",
            "Booking rules", "Student obligations", "Provider obligations", "Exam terms", "Data terms").Value;

    private static IReadOnlyCollection<TrainingContractParty> CreateParties(
        OrganizationId organizationId,
        PersonId studentId) =>
        [
            TrainingContractParty.ForOrganization(
                TrainingContractPartyKind.TrainingProvider, organizationId, "Auto-école Horizon").Value,
            TrainingContractParty.ForPerson(
                TrainingContractPartyKind.Student, studentId, "Student").Value,
        ];
}
