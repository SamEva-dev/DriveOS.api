using DriveOS.Modules.ProfessionalMarketplace.Domain.ServiceEntries;
using DriveOS.SharedKernel.Identifiers;
using Xunit;
namespace DriveOS.UnitTests.Modules.ProfessionalMarketplace;
public sealed class ServiceEntryTests
{
    private static readonly UserId Actor=new(Guid.NewGuid());
    [Fact] public void Entry_cannot_be_outside_engagement_period()
    {
        var r=ServiceEntry.Create(new(Guid.NewGuid()),new(Guid.NewGuid()),null,new(Guid.NewGuid()),new(Guid.NewGuid()),null,
            ServiceEntrySourceType.TrainingSession,Guid.NewGuid(),new DateOnly(2026,10,1),"DRIVING",60,35m,0m,0m,0m,"EUR","Séance conduite",
            new DateOnly(2026,9,1),new DateOnly(2026,9,30),DateTimeOffset.UtcNow,Actor);
        Assert.True(r.IsFailure);
    }
    [Fact] public void Approval_requires_submission()
    {
        var x=ServiceEntry.Create(new(Guid.NewGuid()),new(Guid.NewGuid()),null,new(Guid.NewGuid()),new(Guid.NewGuid()),null,
            ServiceEntrySourceType.TrainingSession,Guid.NewGuid(),new DateOnly(2026,9,10),"DRIVING",90,40m,0m,0m,0m,"EUR","Séance conduite",
            new DateOnly(2026,9,1),new DateOnly(2026,9,30),DateTimeOffset.UtcNow,Actor).Value;
        Assert.True(x.Approve(DateTimeOffset.UtcNow,Actor).IsFailure);
        Assert.True(x.Submit(DateTimeOffset.UtcNow,Actor).IsSuccess);
        Assert.True(x.Approve(DateTimeOffset.UtcNow,Actor).IsSuccess);
        Assert.Equal(60m,x.TotalAmount);
    }
    [Fact] public void Dispute_is_line_scoped()
    {
        var x=ServiceEntry.Create(new(Guid.NewGuid()),new(Guid.NewGuid()),null,new(Guid.NewGuid()),new(Guid.NewGuid()),null,
            ServiceEntrySourceType.MissionActivity,Guid.NewGuid(),new DateOnly(2026,9,10),"ADMIN",30,30m,0m,0m,0m,"EUR","Temps administratif",
            new DateOnly(2026,9,1),new DateOnly(2026,9,30),DateTimeOffset.UtcNow,Actor).Value;
        x.Submit(DateTimeOffset.UtcNow,Actor);
        Assert.True(x.OpenDispute("Durée à vérifier",DateTimeOffset.UtcNow,Actor).IsSuccess);
        Assert.Equal(ServiceEntryStatus.Disputed,x.Status);
    }
}
