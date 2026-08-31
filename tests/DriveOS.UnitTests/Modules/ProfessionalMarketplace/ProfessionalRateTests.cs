using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.SharedKernel.Identifiers;
using Xunit;

namespace DriveOS.UnitTests.Modules.ProfessionalMarketplace;

public sealed class ProfessionalRateTests
{
    private static readonly UserId Actor=new(Guid.NewGuid());
    private static readonly DateTimeOffset Now=DateTimeOffset.UtcNow;

    [Fact]
    public void Rate_currency_is_required_as_iso_like_three_letter_code()
    {
        var p=ProfessionalProfile.Create(new(Guid.NewGuid()),new(Guid.NewGuid()),new(Guid.NewGuid()),null,Now).Value;
        var r=p.ReplaceProfessionalRates([
            new("B_HOURLY",ProfessionalRateUnit.Hour,35m,"EU","B",ProfessionalVehicleProvisionMode.ClientProvided,null,1m,true,new DateOnly(2026,9,1),null)
        ],Now,Actor);
        Assert.True(r.IsFailure);
    }

    [Fact]
    public void Same_rate_code_cannot_have_overlapping_validity_periods()
    {
        var p=ProfessionalProfile.Create(new(Guid.NewGuid()),new(Guid.NewGuid()),new(Guid.NewGuid()),null,Now).Value;
        p.UpdatePresentation("Instructor",null,5,["FR"],["B"],[],Now,Actor);
        var r=p.ReplaceProfessionalRates([
            new("B_HOURLY",ProfessionalRateUnit.Hour,35m,"EUR","B",ProfessionalVehicleProvisionMode.ClientProvided,null,1m,true,new DateOnly(2026,9,1),new DateOnly(2026,12,31)),
            new("B_HOURLY",ProfessionalRateUnit.Hour,38m,"EUR","B",ProfessionalVehicleProvisionMode.ClientProvided,null,1m,true,new DateOnly(2026,12,1),null)
        ],Now,Actor);
        Assert.True(r.IsFailure);
    }

    [Fact]
    public void Rate_is_indicative_and_does_not_create_contractual_terms()
    {
        var rate=new ProfessionalRate("B_HOURLY",ProfessionalRateUnit.Hour,35m,"EUR","B",ProfessionalVehicleProvisionMode.Either,0.45m,1m,true,new DateOnly(2026,9,1),null);
        Assert.True(rate.Negotiable);
        Assert.Equal(35m,rate.Amount);
    }
}
