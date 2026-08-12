using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.Modules.CRM.Domain.Leads.Events;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.CRM.Leads;

public sealed class LeadConversionTests
{
    [Fact]
    public void MarkConverted_WhenLeadIsWonAndQualified_PersistsTargetsAndRaisesEvent()
    {
        Lead lead = CreateQualifiedLead();
        lead.ChangeStatus(LeadStatus.OfferSent).IsSuccess.Should().BeTrue();
        lead.ChangeStatus(LeadStatus.Won).IsSuccess.Should().BeTrue();
        var personId = PersonId.New();
        DraftEnrollmentId enrollmentId = DraftEnrollmentId.New();

        var result = lead.MarkConverted(personId, enrollmentId, DateTimeOffset.UtcNow);

        result.IsSuccess.Should().BeTrue();
        lead.ConvertedPersonId.Should().Be(personId);
        lead.DraftEnrollmentId.Should().Be(enrollmentId);
        lead.DomainEvents.Should().Contain(x => x is ProspectConvertedDomainEvent);
    }

    [Fact]
    public void MarkConverted_WhenLeadIsNotWon_Fails()
    {
        Lead lead = CreateQualifiedLead();

        var result = lead.MarkConverted(PersonId.New(), DraftEnrollmentId.New(), DateTimeOffset.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Crm.Conversions.LeadMustBeWon");
    }

    private static Lead CreateQualifiedLead()
    {
        Lead lead = Lead.Create(LeadId.New(), OrganizationId.New(), null,
            LeadIdentity.Create("John", "Doe", "john@example.com", null).Value,
            RequestedTraining.Create("B", TransmissionPreference.Manual, null).Value,
            LeadSource.Create(LeadSourceType.Website).Value).Value;
        lead.ChangeStatus(LeadStatus.Contacted).IsSuccess.Should().BeTrue();
        lead.Qualify(LeadQualification.Create("Permis B", "B", "Semaine", null,
            FinancingOption.SelfFunded, null).Value).IsSuccess.Should().BeTrue();
        return lead;
    }
}
