using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.SharedKernel.Identifiers;
using Xunit;

namespace DriveOS.UnitTests.Modules.ProfessionalMarketplace;

public sealed class MarketplaceAvailabilityTests
{
    private static readonly UserId Actor=new(Guid.NewGuid());
    private static readonly DateTimeOffset Now=DateTimeOffset.UtcNow;

    [Fact]
    public void Overlapping_recurring_windows_are_rejected()
    {
        var p=ProfessionalProfile.Create(new(Guid.NewGuid()),new(Guid.NewGuid()),new(Guid.NewGuid()),null,Now).Value;
        var r=p.ReplaceMarketplaceAvailability(new MarketplaceAvailabilityPolicy(
            [
                new(DayOfWeek.Monday,new(9,0),new(12,0),"Europe/Paris"),
                new(DayOfWeek.Monday,new(11,0),new(15,0),"Europe/Paris")
            ],[],24,600,300),Now,Actor);
        Assert.True(r.IsFailure);
    }

    [Fact]
    public void All_day_unavailability_exception_is_valid()
    {
        var p=ProfessionalProfile.Create(new(Guid.NewGuid()),new(Guid.NewGuid()),new(Guid.NewGuid()),null,Now).Value;
        var r=p.ReplaceMarketplaceAvailability(new MarketplaceAvailabilityPolicy(
            [new(DayOfWeek.Monday,new(9,0),new(17,0),"Europe/Paris")],
            [new(new DateOnly(2026,9,14),null,null,MarketplaceAvailabilityExceptionType.Unavailable,"Congé")],
            24,600,300),Now,Actor);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public void Commercial_availability_does_not_claim_booking_confirmation()
    {
        var policy=new MarketplaceAvailabilityPolicy([],[],48,480,240);
        Assert.Empty(policy.RecurringRules);
        Assert.Equal(48,policy.MinimumBookingNoticeHours);
    }
}
