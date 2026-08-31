using DriveOS.Modules.CommunicationEngagement.Domain.Surveys;
using DriveOS.SharedKernel.Identifiers;
using Xunit;

namespace DriveOS.UnitTests.Modules.CommunicationEngagement;

public sealed class CommunicationSurveyRequestTests
{
    [Fact]
    public void Partner_feedback_request_preserves_business_context()
    {
        var user=new UserId(Guid.NewGuid());
        var org=new OrganizationId(Guid.NewGuid());
        Guid engagementId=Guid.NewGuid();

        var result=CommunicationSurveyRequest.Create(
            new(Guid.NewGuid()),user,org,"PartnerFeedback",
            $"partner-feedback:first-paid:{engagementId}",
            "PROFESSIONAL_ENGAGEMENT",engagementId,"{}",
            DateTimeOffset.UtcNow);

        Assert.True(result.IsSuccess);
        Assert.Equal("PARTNERFEEDBACK",result.Value.SurveyType);
        Assert.Equal(CommunicationSurveyRequestStatus.Pending,result.Value.Status);
    }
}
