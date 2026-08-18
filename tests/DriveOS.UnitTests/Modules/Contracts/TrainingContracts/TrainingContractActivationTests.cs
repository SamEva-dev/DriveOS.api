using DriveOS.Modules.Contracts.Domain.TrainingContracts;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.Contracts.TrainingContracts;

public sealed class TrainingContractActivationTests
{
    [Fact]
    public void Activate_WhenSignedAndEffective_TransitionsToActive()
    {
        DateTimeOffset now = new(2026, 9, 2, 10, 0, 0, TimeSpan.Zero);
        TrainingContract contract = CreateSignedContract(new DateOnly(2026, 9, 1), new DateOnly(2027, 9, 1), now);
        UserId actor = UserId.New();

        contract.Activate(actor, now).IsSuccess.Should().BeTrue();
        contract.Status.Should().Be(TrainingContractStatus.Active);
        contract.ActivatedAtUtc.Should().Be(now);
        contract.ActivatedByUserId.Should().Be(actor);
    }

    [Fact]
    public void Activate_BeforeStartDate_IsRejected()
    {
        DateTimeOffset now = new(2026, 9, 1, 10, 0, 0, TimeSpan.Zero);
        TrainingContract contract = CreateSignedContract(new DateOnly(2026, 9, 2), null, now);
        contract.Activate(UserId.New(), now).Error.Should().Be(TrainingContractErrors.ActivationBeforeStartDate);
    }

    private static TrainingContract CreateSignedContract(DateOnly startDate, DateOnly? endDate, DateTimeOffset now)
    {
        OrganizationId organizationId = OrganizationId.New();
        PersonId studentId = PersonId.New();
        UserId actor = UserId.New();
        TrainingContract contract = TrainingContract.CreateDraft(
            TrainingContractId.New(), organizationId, BranchId.New(), studentId, CommercialOfferId.New(), 1,
            "CTR-ACT-001", startDate, endDate, 1500m, "EUR", CreateTerms(), CreateParties(organizationId, studentId)).Value;
        TrainingContractSignatory student = contract.AddSignatory(TrainingContractSignatoryKind.Student, studentId, null, "Student", 1, true, null).Value;
        contract.MarkGenerated("doc/ref", "contract.html", "text/html", new string('A', 64), actor, now).IsSuccess.Should().BeTrue();
        contract.MarkSentForSignature(SignatureProcessId.New(), actor, now).IsSuccess.Should().BeTrue();
        contract.RecordSignatorySignature(student.Id, SignatureEvidenceId.New(), actor, now).IsSuccess.Should().BeTrue();
        contract.Status.Should().Be(TrainingContractStatus.Signed);
        return contract;
    }

    private static TrainingContractTermsSnapshot CreateTerms() => TrainingContractTermsSnapshot.Create(
        "B-MANUAL", 20m, "Driving lessons", "3 installments", "Cancellation terms", "Booking rules",
        "Student obligations", "Provider obligations", "Exam terms", "Data terms").Value;

    private static IReadOnlyCollection<TrainingContractParty> CreateParties(OrganizationId organizationId, PersonId studentId) =>
    [
        TrainingContractParty.ForOrganization(TrainingContractPartyKind.TrainingProvider, organizationId, "Auto-école Horizon").Value,
        TrainingContractParty.ForPerson(TrainingContractPartyKind.Student, studentId, "Student").Value,
    ];
}
