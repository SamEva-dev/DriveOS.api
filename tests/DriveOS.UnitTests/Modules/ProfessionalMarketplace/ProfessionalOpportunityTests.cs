using DriveOS.Modules.ProfessionalMarketplace.Domain.Opportunities;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.SharedKernel.Identifiers;
using Xunit;

namespace DriveOS.UnitTests.Modules.ProfessionalMarketplace;

public sealed class ProfessionalOpportunityTests
{
    private static readonly UserId Actor=new(Guid.NewGuid());
    private static readonly OrganizationId Organization=new(Guid.NewGuid());
    private static readonly DateTimeOffset Now=new(2026,8,25,4,30,0,TimeSpan.Zero);

    private static ProfessionalOpportunity CreateDraft()=>ProfessionalOpportunity.Create(
        new(Guid.NewGuid()),Organization,null,"Besoin enseignant B","Remplacement ponctuel",
        ProfessionalType.DrivingInstructor,["B"],["FR"],["AAC"],"FR","NICE","Nice",43.710m,7.261m,20,
        new DateOnly(2026,9,1),new DateOnly(2026,9,30),
        [new(DayOfWeek.Tuesday,new(9,0),new(17,0),"Europe/Paris")],2400,
        ProfessionalEngagementType.FixedMission,ProfessionalVehicleProvisionMode.Either,
        30m,40m,"EUR",ProfessionalRateUnit.Hour,true,Now,Actor).Value;

    [Fact]
    public void Opportunity_starts_as_draft_and_can_be_published()
    {
        var x=CreateDraft();
        Assert.Equal(ProfessionalOpportunityStatus.Draft,x.Status);
        Assert.True(x.Publish(Now,Actor).IsSuccess);
        Assert.Equal(ProfessionalOpportunityStatus.Published,x.Status);
    }

    [Fact]
    public void Overlapping_time_windows_are_rejected()
    {
        var r=ProfessionalOpportunity.Create(
            new(Guid.NewGuid()),Organization,null,"Besoin enseignant","Mission",
            ProfessionalType.DrivingInstructor,["B"],[],[],"FR","NICE","Nice",43.710m,7.261m,20,
            new DateOnly(2026,9,1),new DateOnly(2026,9,30),
            [new(DayOfWeek.Monday,new(9,0),new(12,0),"Europe/Paris"),new(DayOfWeek.Monday,new(11,0),new(15,0),"Europe/Paris")],
            null,ProfessionalEngagementType.FixedMission,ProfessionalVehicleProvisionMode.ClientProvided,
            null,null,null,null,true,Now,Actor);
        Assert.True(r.IsFailure);
    }

    [Fact]
    public void Budget_range_must_be_consistent()
    {
        var r=ProfessionalOpportunity.Create(
            new(Guid.NewGuid()),Organization,null,"Besoin enseignant","Mission",
            ProfessionalType.DrivingInstructor,["B"],[],[],"FR",null,null,null,null,null,
            new DateOnly(2026,9,1),new DateOnly(2026,9,30),[],null,
            ProfessionalEngagementType.FixedMission,ProfessionalVehicleProvisionMode.ClientProvided,
            50m,30m,"EUR",ProfessionalRateUnit.Hour,false,Now,Actor);
        Assert.True(r.IsFailure);
    }

    [Fact]
    public void Tenant_owned_opportunity_carries_organization_id()
    {
        var x=CreateDraft();
        Assert.Equal(Organization,x.OrganizationId);
    }
}
