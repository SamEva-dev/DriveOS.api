using DriveOS.Modules.CRM.Domain.Conversions;
using DriveOS.Modules.CRM.Domain.Conversions.Events;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.CRM.Conversions;

public sealed class LeadConversionRequestTests
{
    [Fact]
    public void Request_KeepsAcceptedOfferAndRaisesIntegrationBoundaryEvent()
    {
        OrganizationId organizationId = OrganizationId.New();
        Lead lead = Lead.Create(LeadId.New(), organizationId, BranchId.New(),
            LeadIdentity.Create("Jane", "Doe", "jane@example.com", null).Value,
            RequestedTraining.Create("B", TransmissionPreference.Manual, null).Value,
            LeadSource.Create(LeadSourceType.Website).Value).Value;
        CommercialOfferId offerId = CommercialOfferId.New();

        LeadConversion conversion = LeadConversion.Request(organizationId, lead, offerId,
            BranchId.New(), UserId.New(), "B", true, true, true, null, null, "ID_CARD");

        conversion.Status.Should().Be(LeadConversionStatus.Requested);
        conversion.AcceptedOfferId.Should().Be(offerId);
        conversion.StudentPersonId.Should().BeNull();
        conversion.StudentEnrollmentId.Should().BeNull();
        conversion.DomainEvents.Should().ContainSingle(x => x is LeadConversionRequestedDomainEvent);
    }
}
