using DriveOS.Modules.ProfessionalMarketplace.Domain.StudentAssignments;
using DriveOS.SharedKernel.Identifiers;
using Xunit;

namespace DriveOS.UnitTests.Modules.ProfessionalMarketplace;

public sealed class ProfessionalStudentAssignmentTests
{
    private static readonly UserId Actor=new(Guid.NewGuid());

    [Fact]
    public void Assignment_must_stay_inside_mission_period()
    {
        var r=ProfessionalStudentAssignment.Create(
            new(Guid.NewGuid()),new(Guid.NewGuid()),new(Guid.NewGuid()),new(Guid.NewGuid()),
            new(Guid.NewGuid()),new PersonId(Guid.NewGuid()),
            new DateOnly(2026,9,1),new DateOnly(2026,9,20),"TRAINING",Actor,"Continuité pédagogique",
            new DateOnly(2026,9,5),new DateOnly(2026,9,30),DateTimeOffset.UtcNow,Actor);

        Assert.True(r.IsFailure);
    }

    [Fact]
    public void Active_assignment_can_be_revoked()
    {
        var x=ProfessionalStudentAssignment.Create(
            new(Guid.NewGuid()),new(Guid.NewGuid()),new(Guid.NewGuid()),new(Guid.NewGuid()),
            new(Guid.NewGuid()),new PersonId(Guid.NewGuid()),
            new DateOnly(2026,9,5),new DateOnly(2026,9,20),"TRAINING",Actor,"Continuité pédagogique",
            new DateOnly(2026,9,1),new DateOnly(2026,9,30),DateTimeOffset.UtcNow,Actor).Value;

        Assert.True(x.Revoke("Changement de moniteur",DateTimeOffset.UtcNow,Actor).IsSuccess);
        Assert.Equal(ProfessionalStudentAssignmentStatus.Revoked,x.Status);
    }
}
