using DriveOS.Modules.ProfessionalMarketplace.Domain.AccessGrants;
using DriveOS.Modules.ProfessionalMarketplace.Domain.StudentAssignments;
using DriveOS.SharedKernel.Identifiers;
using Xunit;

namespace DriveOS.UnitTests.Modules.ProfessionalMarketplace;

public sealed class EngagementClosureTests
{
    private static readonly UserId Actor=new(Guid.NewGuid());

    [Fact]
    public void Active_student_assignment_can_be_revoked_on_engagement_closure()
    {
        var assignment=ProfessionalStudentAssignment.Create(
            new(Guid.NewGuid()),new(Guid.NewGuid()),new(Guid.NewGuid()),new(Guid.NewGuid()),
            new(Guid.NewGuid()),new PersonId(Guid.NewGuid()),
            new DateOnly(2026,9,1),new DateOnly(2026,9,30),"TRAINING",Actor,"Continuité pédagogique",
            new DateOnly(2026,9,1),new DateOnly(2026,9,30),
            DateTimeOffset.UtcNow,Actor).Value;

        Assert.True(assignment.Revoke("Engagement terminated",DateTimeOffset.UtcNow,Actor).IsSuccess);
        Assert.Equal(ProfessionalStudentAssignmentStatus.Revoked,assignment.Status);
    }

    [Fact]
    public void Active_external_access_is_revoked_not_deleted()
    {
        var grant=ExternalAccessGrant.Create(
            new(Guid.NewGuid()),new(Guid.NewGuid()),new(Guid.NewGuid()),new(Guid.NewGuid()),null,
            ExternalAccessResourceType.Student,Guid.NewGuid(),"READ",
            new DateOnly(2026,9,1),new DateOnly(2026,9,30),
            new DateOnly(2026,9,1),new DateOnly(2026,9,30),
            DateTimeOffset.UtcNow,Actor).Value;

        Assert.True(grant.Revoke("Engagement terminated",DateTimeOffset.UtcNow,Actor).IsSuccess);
        Assert.Equal(ExternalAccessGrantStatus.Revoked,grant.Status);
        Assert.NotNull(grant.RevokedAtUtc);
    }
}
