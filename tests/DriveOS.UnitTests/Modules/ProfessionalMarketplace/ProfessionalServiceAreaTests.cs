using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.SharedKernel.Identifiers;
using Xunit;

namespace DriveOS.UnitTests.Modules.ProfessionalMarketplace;

public sealed class ProfessionalServiceAreaTests
{
    private static readonly UserId Actor=new(Guid.NewGuid());
    private static readonly DateTimeOffset Now=DateTimeOffset.UtcNow;

    [Fact]
    public void Exactly_one_primary_area_is_required()
    {
        var p=ProfessionalProfile.Create(new(Guid.NewGuid()),new(Guid.NewGuid()),new(Guid.NewGuid()),null,Now).Value;
        var r=p.ReplaceServiceAreas([
            new("NICE","FR","Nice",43.71m,7.26m,20,false,ProfessionalMobilityMode.Radius)
        ],Now,Actor);
        Assert.True(r.IsFailure);
    }

    [Fact]
    public void Precise_private_address_is_not_part_of_service_area()
    {
        var area=new ProfessionalServiceArea("NICE","FR","Nice",43.71m,7.26m,20,true,ProfessionalMobilityMode.Radius);
        Assert.DoesNotContain("Rue",area.DisplayName,StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Coordinates_are_reduced_to_approximate_precision()
    {
        var p=ProfessionalProfile.Create(new(Guid.NewGuid()),new(Guid.NewGuid()),new(Guid.NewGuid()),null,Now).Value;
        var r=p.ReplaceServiceAreas([
            new("NICE","FR","Nice",43.710123m,7.261234m,20,true,ProfessionalMobilityMode.Radius)
        ],Now,Actor);
        Assert.True(r.IsSuccess);
        Assert.Equal(43.710m,p.ServiceAreas[0].Latitude);
        Assert.Equal(7.261m,p.ServiceAreas[0].Longitude);
    }
}
