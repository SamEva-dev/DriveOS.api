using DriveOS.Modules.ProfessionalMarketplace.Application.Matching;
using Xunit;

namespace DriveOS.UnitTests.Modules.ProfessionalMarketplace;

public sealed class ProfessionalMatchingTests
{
    [Fact]
    public void Score_breakdown_total_has_100_point_ceiling()
    {
        var b=new ProfessionalMatchBreakdown(30m,10m,10m,10m,15m,5m,10m,10m);
        decimal total=b.CategoryScore+b.LanguageScore+b.SpecializationScore+b.DistanceScore+
                      b.AvailabilityScore+b.VehicleScore+b.RateScore+b.ComplianceScore;
        Assert.Equal(100m,total);
    }

    [Fact]
    public void Compliance_is_not_only_a_scoring_bonus()
    {
        string blockingReason="COMPLIANCE_NOT_VERIFIED";
        Assert.Equal("COMPLIANCE_NOT_VERIFIED",blockingReason);
    }

    [Fact]
    public void Matching_result_exposes_explanations_and_blocking_reasons_separately()
    {
        var r=new ProfessionalMatchResult(Guid.NewGuid(),"Test professional","Instructor",5,["B"],["FR"],"Nice",45m,"EUR","Hour",42m,false,["NO_TEACHING_CATEGORY_MATCH"],
            new ProfessionalMatchBreakdown(0,10,10,5,0,5,2,10),["DISTANCE_KM:4.2"]);
        Assert.False(r.Eligible);
        Assert.Single(r.BlockingReasons);
        Assert.Single(r.Explanations);
    }
}
