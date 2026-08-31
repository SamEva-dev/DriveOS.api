using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.SharedKernel.Identifiers;
using Xunit;

namespace DriveOS.UnitTests.Modules.ProfessionalMarketplace;

public sealed class ProfessionalSearchTests
{
    private static readonly UserId Actor=new(Guid.NewGuid());
    private static readonly DateTimeOffset Now=new(2026,8,24,20,0,0,TimeSpan.Zero);

    [Fact]
    public void Marketplace_profile_remains_non_discoverable_until_active_and_verified()
    {
        var p=ProfessionalProfile.Create(new(Guid.NewGuid()),new(Guid.NewGuid()),new(Guid.NewGuid()),null,Now).Value;
        Assert.False(p.IsDiscoverable);
    }

    [Fact]
    public void Rate_can_be_filtered_by_teaching_category_and_effective_date()
    {
        var rate=new ProfessionalRate(
            "B_HOURLY",ProfessionalRateUnit.Hour,35m,"EUR","B",
            ProfessionalVehicleProvisionMode.Either,null,1m,true,
            new DateOnly(2026,9,1),new DateOnly(2026,12,31));

        Assert.Equal("B",rate.TeachingCategoryCode);
        Assert.True(rate.EffectiveFrom<=new DateOnly(2026,10,1));
        Assert.True(rate.EffectiveTo>=new DateOnly(2026,10,1));
    }

    [Fact]
    public void Availability_exception_can_override_recurring_commercial_availability()
    {
        var policy=new MarketplaceAvailabilityPolicy(
            [new(DayOfWeek.Monday,new(9,0),new(17,0),"Europe/Paris")],
            [new(new DateOnly(2026,9,14),null,null,MarketplaceAvailabilityExceptionType.Unavailable,"Congé")],
            24,600,300);

        Assert.Single(policy.RecurringRules);
        Assert.Equal(MarketplaceAvailabilityExceptionType.Unavailable,policy.Exceptions[0].Type);
    }
}
