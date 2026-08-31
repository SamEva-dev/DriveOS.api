using DriveOS.Modules.ProfessionalMarketplace.Application.Engagements;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Engagements;
using DriveOS.SharedKernel.Identifiers;
using Xunit;

namespace DriveOS.UnitTests.Modules.ProfessionalMarketplace;

public sealed class ProfessionalSchedulingPreparationTests
{
    [Fact]
    public void Preparation_result_distinguishes_failure_from_created_resource()
    {
        var failed=new ProfessionalSchedulingPreparationResult(false,null,"professional-marketplace.scheduling.invalid-time-zone");
        var prepared=new ProfessionalSchedulingPreparationResult(true,Guid.NewGuid(),null);

        Assert.False(failed.IsPrepared);
        Assert.True(prepared.IsPrepared);
        Assert.NotNull(prepared.CalendarResourceId);
    }

    [Fact]
    public void Scheduling_is_an_explicit_preparation_step()
    {
        Assert.Contains(EngagementPreparationStep.Scheduling,Enum.GetValues<EngagementPreparationStep>());
    }

    [Fact]
    public void Request_carries_operational_scope_needed_by_scheduling()
    {
        var request=new ProfessionalSchedulingPreparationRequest(
            new OrganizationId(Guid.NewGuid()),
            new BranchId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()),
            "Jean Dupont",
            "Europe/Paris",
            ["B"],
            new DateOnly(2026,9,1),
            new DateOnly(2026,9,30));

        Assert.Single(request.TeachingCategoryCodes);
        Assert.Equal("B",request.TeachingCategoryCodes[0]);
        Assert.True(request.StartsOn<=request.EndsOn);
    }
}
