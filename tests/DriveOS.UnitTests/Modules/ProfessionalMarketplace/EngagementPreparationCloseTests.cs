using DriveOS.Modules.ProfessionalMarketplace.Domain.Engagements;
using Xunit;

namespace DriveOS.UnitTests.Modules.ProfessionalMarketplace;

public sealed class EngagementPreparationCloseTests
{
    [Fact]
    public void Internal_approval_is_part_of_operational_readiness()
    {
        Assert.Contains(EngagementPreparationStep.InternalApproval,
            Enum.GetValues<EngagementPreparationStep>());
    }
}
