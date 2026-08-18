using DriveOS.Modules.FundingBilling.Domain.Refunds;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;
using Xunit;

namespace DriveOS.UnitTests.Modules.FundingBilling.Refunds;

public sealed class RefundTests
{
    private static readonly OrganizationId Org = new(Guid.NewGuid());
    private static readonly BillingAccountId Account = new(Guid.NewGuid());
    private static readonly PaymentId Payment = new(Guid.NewGuid());
    private static readonly UserId User = new(Guid.NewGuid());

    [Fact]
    public void Request_then_approve_then_complete_should_follow_lifecycle()
    {
        var now=DateTimeOffset.UtcNow; var created=Refund.Request(RefundId.New(),Org,Account,Payment,50m,"EUR","Trop perçu",User,now);
        created.IsSuccess.Should().BeTrue(); created.Value.Status.Should().Be(RefundStatus.Requested);
        created.Value.Approve(User,now.AddMinutes(1)).IsSuccess.Should().BeTrue();
        created.Value.Complete("rf_123",User,now.AddMinutes(2)).IsSuccess.Should().BeTrue();
        created.Value.Status.Should().Be(RefundStatus.Completed); created.Value.ProviderReference.Should().Be("rf_123");
    }

    [Fact]
    public void Complete_without_approval_should_fail()
    {
        var now=DateTimeOffset.UtcNow; var refund=Refund.Request(RefundId.New(),Org,Account,Payment,20m,"EUR","Correction",User,now).Value;
        refund.Complete(null,User,now.AddMinutes(1)).IsFailure.Should().BeTrue();
    }
}
