using DriveOS.Modules.Contracts.Domain.TrainingContracts;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.Contracts.TrainingContracts;

public sealed class TrainingContractSuspensionTests
{
    [Fact]
    public void Suspend_Should_Reject_Draft_Contract()
    {
        var organizationId = new OrganizationId(Guid.NewGuid());
        var studentId = new PersonId(Guid.NewGuid());
        TrainingContractTermsSnapshot terms = TrainingContractTermsSnapshot.Create(
            "B", 20m, "services", "payment", "cancel", "booking", "student", "provider", "exam", "data").Value;
        TrainingContract contract = TrainingContract.CreateDraft(
            new TrainingContractId(Guid.NewGuid()), organizationId, new BranchId(Guid.NewGuid()), studentId,
            new CommercialOfferId(Guid.NewGuid()), 1, "CON-TEST", DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(6)), 1200m, "EUR", terms,
            [
                TrainingContractParty.ForOrganization(TrainingContractPartyKind.TrainingProvider, organizationId, "Provider", null).Value,
                TrainingContractParty.ForPerson(TrainingContractPartyKind.Student, studentId, "Student", null).Value
            ]).Value;

        var result = contract.Suspend(
            "Temporary administrative suspension", DateOnly.FromDateTime(DateTime.UtcNow), null,
            new UserId(Guid.NewGuid()), DateTimeOffset.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Contracts.TrainingContract.Suspension.NotAllowed");
    }
}
