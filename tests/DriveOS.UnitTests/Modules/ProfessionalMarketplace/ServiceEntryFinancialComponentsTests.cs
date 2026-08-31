using DriveOS.Modules.ProfessionalMarketplace.Domain.ServiceEntries;
using DriveOS.SharedKernel.Identifiers;
using Xunit;

namespace DriveOS.UnitTests.Modules.ProfessionalMarketplace;

public sealed class ServiceEntryFinancialComponentsTests
{
    [Fact]
    public void Total_includes_expenses_and_indemnities_and_subtracts_discount()
    {
        UserId actor=new(Guid.NewGuid());
        var entry=ServiceEntry.Create(
            new(Guid.NewGuid()),new(Guid.NewGuid()),null,new(Guid.NewGuid()),new(Guid.NewGuid()),null,
            ServiceEntrySourceType.TrainingSession,Guid.NewGuid(),new DateOnly(2026,9,10),
            "DRIVING",60,40m,10m,5m,3m,"EUR","Séance conduite",
            new DateOnly(2026,9,1),new DateOnly(2026,9,30),DateTimeOffset.UtcNow,actor).Value;

        Assert.Equal(40m,entry.BaseAmount);
        Assert.Equal(52m,entry.TotalAmount);
    }
}
