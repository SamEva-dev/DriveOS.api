using DriveOS.Modules.Contracts.Domain.ContractAmendments;
using DriveOS.Modules.Contracts.Domain.TrainingContracts;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.Contracts.ContractAmendments;

public sealed class ContractAmendmentTests
{
    [Fact]
    public void SignedAmendment_WhenApplied_CreatesNewContractVersionAndKeepsHistory()
    {
        DateTimeOffset now = new(2026, 9, 2, 10, 0, 0, TimeSpan.Zero);
        OrganizationId organizationId = OrganizationId.New();
        PersonId studentId = PersonId.New();
        UserId actor = UserId.New();
        TrainingContract contract = CreateActiveContract(organizationId, studentId, actor, now);
        TrainingContractTermsSnapshot amendedTerms = CreateTerms(24m, "24 practical hours");

        ContractAmendment amendment = ContractAmendment.CreateDraft(
            ContractAmendmentId.New(), organizationId, contract.Id, 1, contract.CurrentVersionNumber,
            "Additional practical training hours", new DateOnly(2026, 9, 2), contract.StartDate, contract.EndDate,
            1750m, "EUR", amendedTerms).Value;

        amendment.MarkSigned("documents/amendment-1.pdf", new string('B', 64), actor, now).IsSuccess.Should().BeTrue();
        int version = contract.ApplySignedAmendment(amendment, actor, now).Value;
        amendment.MarkApplied(version, actor, now).IsSuccess.Should().BeTrue();

        contract.Status.Should().Be(TrainingContractStatus.Amended);
        contract.CurrentVersionNumber.Should().Be(2);
        contract.Versions.Should().HaveCount(2);
        contract.Versions.Should().Contain(x => x.VersionNumber == 1 && x.TotalAmount == 1500m);
        contract.TotalAmount.Should().Be(1750m);
        contract.TermsSnapshot.PracticalHours.Should().Be(24m);
        amendment.Status.Should().Be(ContractAmendmentStatus.Applied);
    }

    [Fact]
    public void Apply_WhenBaseVersionChanged_IsRejected()
    {
        DateTimeOffset now = new(2026, 9, 2, 10, 0, 0, TimeSpan.Zero);
        OrganizationId organizationId = OrganizationId.New();
        PersonId studentId = PersonId.New();
        UserId actor = UserId.New();
        TrainingContract contract = CreateActiveContract(organizationId, studentId, actor, now);
        ContractAmendment amendment = ContractAmendment.CreateDraft(
            ContractAmendmentId.New(), organizationId, contract.Id, 1, 99,
            "Change payment schedule only", new DateOnly(2026, 9, 2), contract.StartDate, contract.EndDate,
            contract.TotalAmount, contract.Currency, contract.TermsSnapshot).Value;
        amendment.MarkSigned("documents/amendment.pdf", new string('C', 64), actor, now).IsSuccess.Should().BeTrue();

        contract.ApplySignedAmendment(amendment, actor, now).Error.Should().Be(ContractAmendmentErrors.BaseVersionChanged);
    }

    private static TrainingContract CreateActiveContract(OrganizationId organizationId, PersonId studentId, UserId actor, DateTimeOffset now)
    {
        TrainingContract contract = TrainingContract.CreateDraft(
            TrainingContractId.New(), organizationId, BranchId.New(), studentId, CommercialOfferId.New(), 1,
            "CTR-AMD-001", new DateOnly(2026, 9, 1), new DateOnly(2027, 9, 1), 1500m, "EUR",
            CreateTerms(20m, "20 practical hours"), CreateParties(organizationId, studentId)).Value;
        TrainingContractSignatory student = contract.AddSignatory(TrainingContractSignatoryKind.Student, studentId, null, "Student", 1, true, null).Value;
        contract.MarkGenerated("doc/ref", "contract.html", "text/html", new string('A', 64), actor, now).IsSuccess.Should().BeTrue();
        contract.MarkSentForSignature(SignatureProcessId.New(), actor, now).IsSuccess.Should().BeTrue();
        contract.RecordSignatorySignature(student.Id, SignatureEvidenceId.New(), actor, now).IsSuccess.Should().BeTrue();
        contract.Activate(actor, now).IsSuccess.Should().BeTrue();
        return contract;
    }

    private static TrainingContractTermsSnapshot CreateTerms(decimal hours, string services) => TrainingContractTermsSnapshot.Create(
        "B-MANUAL", hours, services, "3 installments", "Cancellation terms", "Booking rules",
        "Student obligations", "Provider obligations", "Exam terms", "Data terms").Value;

    private static IReadOnlyCollection<TrainingContractParty> CreateParties(OrganizationId organizationId, PersonId studentId) =>
    [
        TrainingContractParty.ForOrganization(TrainingContractPartyKind.TrainingProvider, organizationId, "Auto-école Horizon").Value,
        TrainingContractParty.ForPerson(TrainingContractPartyKind.Student, studentId, "Student").Value,
    ];
}
