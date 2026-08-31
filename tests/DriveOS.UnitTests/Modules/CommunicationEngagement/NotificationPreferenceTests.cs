using DriveOS.Modules.CommunicationEngagement.Domain.Notifications;
using DriveOS.SharedKernel.Identifiers;
using Xunit;

namespace DriveOS.UnitTests.Modules.CommunicationEngagement;

public sealed class NotificationPreferenceTests
{
    [Fact]
    public void User_can_disable_email_without_disabling_in_app()
    {
        UserId user=new(Guid.NewGuid());
        var pref=NotificationPreference.Create(
            new(Guid.NewGuid()),user,"MISSION",true,false,DateTimeOffset.UtcNow).Value;

        Assert.True(pref.InAppEnabled);
        Assert.False(pref.EmailEnabled);
    }

    [Fact]
    public void Category_is_normalized()
    {
        var pref=NotificationPreference.Create(
            new(Guid.NewGuid()),new UserId(Guid.NewGuid())," supplier_payment ",true,true,DateTimeOffset.UtcNow).Value;

        Assert.Equal("SUPPLIER_PAYMENT",pref.Category);
    }
}
