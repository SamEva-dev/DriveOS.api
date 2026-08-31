using DriveOS.Modules.ProfessionalMarketplace.Domain.ServiceEntries;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ServiceStatements;
using DriveOS.SharedKernel.Identifiers;
using Xunit;

namespace DriveOS.UnitTests.Modules.ProfessionalMarketplace;

public sealed class ServiceStatementTests
{
    private static readonly UserId Actor=new(Guid.NewGuid());
    private static readonly DateTimeOffset Now=DateTimeOffset.UtcNow;

    private static ServiceEntry Entry(ServiceEntryStatus target)
    {
        var x=ServiceEntry.Create(new(Guid.NewGuid()),new(Guid.NewGuid()),null,new(Guid.NewGuid()),new(Guid.NewGuid()),null,
            ServiceEntrySourceType.TrainingSession,Guid.NewGuid(),new DateOnly(2026,9,10),"DRIVING",60,40m,0m,0m,0m,"EUR","Séance conduite",
            new DateOnly(2026,9,1),new DateOnly(2026,9,30),Now,Actor).Value;
        x.Submit(Now,Actor);
        if(target==ServiceEntryStatus.Approved)x.Approve(Now,Actor);
        if(target==ServiceEntryStatus.Disputed)x.OpenDispute("Durée à contrôler",Now,Actor);
        if(target==ServiceEntryStatus.Rejected)x.Reject("Prestation invalide",Now,Actor);
        return x;
    }

    [Fact]
    public void Statement_rejects_mixed_currencies()
    {
        var a=Entry(ServiceEntryStatus.Submitted);
        var b=ServiceEntry.Create(new(Guid.NewGuid()),a.EngagementId,null,a.ProfessionalProfileId,a.OrganizationId,null,
            ServiceEntrySourceType.MissionActivity,Guid.NewGuid(),new DateOnly(2026,9,11),"ADMIN",60,30m,0m,0m,0m,"USD","Temps administratif",
            new DateOnly(2026,9,1),new DateOnly(2026,9,30),Now,Actor).Value;
        b.Submit(Now,Actor);

        var r=ServiceStatement.Create(new(Guid.NewGuid()),a.EngagementId,a.ProfessionalProfileId,a.OrganizationId,Guid.NewGuid(),
            new DateOnly(2026,9,1),new DateOnly(2026,9,30),[a,b],Now,Actor);

        Assert.True(r.IsFailure);
    }

    [Fact]
    public void Disputed_line_does_not_remove_approved_amount()
    {
        var approved=Entry(ServiceEntryStatus.Approved);
        var disputed=Entry(ServiceEntryStatus.Disputed);
        typeof(ServiceEntry).GetProperty(nameof(ServiceEntry.EngagementId))!
            .SetValue(disputed,approved.EngagementId);

        var statement=ServiceStatement.Create(new(Guid.NewGuid()),approved.EngagementId,approved.ProfessionalProfileId,approved.OrganizationId,
            Guid.NewGuid(),new DateOnly(2026,9,1),new DateOnly(2026,9,30),[approved,disputed],Now,Actor).Value;

        statement.Submit(Now,Actor);
        statement.StartReview(Now,Actor);
        statement.RecalculateReviewStatus(Now,Actor);

        Assert.Equal(ServiceStatementStatus.PartiallyApproved,statement.Status);
        Assert.Equal(approved.TotalAmount,statement.ApprovedAmount);
        Assert.True(statement.DisputedAmount>0);
    }
    [Fact]
    public void Partially_approved_statement_cannot_be_rejected_as_a_whole()
    {
        var approved=Entry(ServiceEntryStatus.Approved);
        var submitted=Entry(ServiceEntryStatus.Submitted);
        typeof(ServiceEntry).GetProperty(nameof(ServiceEntry.EngagementId))!
            .SetValue(submitted,approved.EngagementId);

        var statement=ServiceStatement.Create(new(Guid.NewGuid()),approved.EngagementId,approved.ProfessionalProfileId,approved.OrganizationId,
            Guid.NewGuid(),new DateOnly(2026,9,1),new DateOnly(2026,9,30),[approved,submitted],Now,Actor).Value;
        statement.Submit(Now,Actor);
        statement.StartReview(Now,Actor);
        statement.RecalculateReviewStatus(Now,Actor);

        var rejected=statement.Reject("Rejet global",Now,Actor);

        Assert.True(rejected.IsFailure);
        Assert.Equal(ServiceStatementStatus.PartiallyApproved,statement.Status);
    }

}
