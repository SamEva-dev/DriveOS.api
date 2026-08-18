using DriveOS.Modules.FundingBilling.Domain.BillingParties;
using DriveOS.SharedKernel.Identifiers;
using Xunit;

namespace DriveOS.UnitTests.Modules.FundingBilling.BillingParties;

public sealed class BillingPartyTests
{
    [Fact]
    public void Create_ShouldRequireExactlyOnePartyIdentity()
    {
        var result = BillingParty.Create(BillingPartyId.New(), new OrganizationId(Guid.NewGuid()), new BillingAccountId(Guid.NewGuid()), new PersonId(Guid.NewGuid()), new OrganizationId(Guid.NewGuid()), BillingPartyRole.Payer, null, DateOnly.FromDateTime(DateTime.UtcNow), null, 10, true);
        Assert.True(result.IsFailure); Assert.Equal(BillingPartyErrors.InvalidParty.Code, result.Error.Code);
    }

    [Fact]
    public void Create_PayerAndFunder_ShouldExposeBothCapabilities()
    {
        var result = BillingParty.Create(BillingPartyId.New(), new OrganizationId(Guid.NewGuid()), new BillingAccountId(Guid.NewGuid()), new PersonId(Guid.NewGuid()), null, BillingPartyRole.PayerAndFunder, 1500m, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), 1, true);
        Assert.True(result.IsSuccess); Assert.True(result.Value.CanPay); Assert.True(result.Value.CanFund); Assert.Equal(1500m, result.Value.MaximumAmount);
    }

    [Fact]
    public void End_ShouldDisableEffectiveRelationship()
    {
        var result = BillingParty.Create(BillingPartyId.New(), new OrganizationId(Guid.NewGuid()), new BillingAccountId(Guid.NewGuid()), null, new OrganizationId(Guid.NewGuid()), BillingPartyRole.Funder, null, new DateOnly(2026, 1, 1), null, 10, false);
        Assert.True(result.IsSuccess);
        Assert.True(result.Value.End("Funding ended", new UserId(Guid.NewGuid()), DateTimeOffset.UtcNow).IsSuccess);
        Assert.Equal(BillingPartyStatus.Ended, result.Value.Status); Assert.False(result.Value.IsEffectiveOn(new DateOnly(2026, 8, 18)));
    }
}
