using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;
using Xunit;

namespace DriveOS.UnitTests.Modules.CRM.Leads;

public sealed class LeadAssignmentTests
{
    [Fact]
    public void AssignAdvisor_changes_the_owner_through_the_aggregate()
    {
        UserId advisor = UserId.New();
        Lead lead = CreateLead();

        var result = lead.AssignAdvisor(advisor);

        Assert.True(result.IsSuccess);
        Assert.Equal(advisor, lead.AssignedAdvisorId);
    }

    [Fact]
    public void AssignAdvisor_rejects_an_empty_identifier()
    {
        Lead lead = CreateLead();
        var result = lead.AssignAdvisor(UserId.Empty);
        Assert.True(result.IsFailure);
        Assert.Equal("Crm.Leads.AssignedAdvisorId.Empty", result.Error.Code);
    }

    private static Lead CreateLead()
    {
        var identity = LeadIdentity
            .Create("Ada", "Lovelace", "ada@example.test", "+33123456789")
            .Value;
        var training = RequestedTraining.Create("B", TransmissionPreference.Manual, null).Value;
        var source = LeadSource.Create(LeadSourceType.Website, null).Value;
        return Lead.Create(
            LeadId.New(),
            OrganizationId.New(),
            null,
            identity,
            training,
            source
        ).Value;
    }
}
