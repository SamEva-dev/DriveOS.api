using DriveOS.Modules.Workforce.Domain.ProfessionalRestrictions;using DriveOS.SharedKernel.Identifiers;using FluentAssertions;using Xunit;
namespace DriveOS.UnitTests.Workforce;
public sealed class ProfessionalRestrictionTests
{
 [Fact] public void Active_restriction_can_be_lifted_but_not_edited(){var actor=new UserId(Guid.NewGuid());var r=ProfessionalRestriction.Create(ProfessionalRestrictionId.New(),new OrganizationId(Guid.NewGuid()),new EmployeeId(Guid.NewGuid()),ProfessionalRestrictionActivity.Teaching,ProfessionalRestrictionSource.InternalDecision,new DateOnly(2026,9,1),null,"Temporary restriction","FR","B",null,null,DateTimeOffset.UtcNow,actor).Value;r.Activate(DateTimeOffset.UtcNow,actor).IsSuccess.Should().BeTrue();r.UpdatePlan(new DateOnly(2026,9,2),null,"x","FR","B",null,null,DateTimeOffset.UtcNow,actor).IsFailure.Should().BeTrue();r.Lift("Restriction removed",DateTimeOffset.UtcNow,actor).IsSuccess.Should().BeTrue();}
 [Fact] public void Invalid_period_is_rejected(){var r=ProfessionalRestriction.Create(ProfessionalRestrictionId.New(),new OrganizationId(Guid.NewGuid()),new EmployeeId(Guid.NewGuid()),ProfessionalRestrictionActivity.Teaching,ProfessionalRestrictionSource.InternalDecision,new DateOnly(2026,9,10),new DateOnly(2026,9,1),"reason",null,null,null,null,DateTimeOffset.UtcNow,new UserId(Guid.NewGuid()));r.IsFailure.Should().BeTrue();}
}
