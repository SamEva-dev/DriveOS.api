using DriveOS.Modules.CommunicationEngagement.Domain.Notifications;
using DriveOS.SharedKernel.Identifiers;
using Xunit;

namespace DriveOS.UnitTests.Modules.CommunicationEngagement;

public sealed class CommunicationNotificationTests
{
    private static readonly UserId Actor=new(Guid.NewGuid());

    [Fact]
    public void Notification_requires_stable_deduplication_key()
    {
        var r=CommunicationNotification.Create(
            new(Guid.NewGuid()),
            CommunicationNotificationRecipientType.User,
            Guid.NewGuid(),
            new OrganizationId(Guid.NewGuid()),
            "MISSION",
            "professionalMarketplace.notifications.missionProposed",
            "",
            "{}",
            "PROFESSIONAL_MISSION",
            Guid.NewGuid(),
            "pro@example.test",
            "fr",
            true,
            DateTimeOffset.UtcNow,
            Actor);

        Assert.True(r.IsFailure);
    }

    [Fact]
    public void Unread_notification_can_be_marked_read()
    {
        var notification=CommunicationNotification.Create(
            new(Guid.NewGuid()),
            CommunicationNotificationRecipientType.User,
            Guid.NewGuid(),
            null,
            "PAYMENT",
            "professionalMarketplace.notifications.paymentReceived",
            $"payment-paid:{Guid.NewGuid()}",
            "{}",
            "SUPPLIER_INVOICE",
            Guid.NewGuid(),
            "pro@example.test",
            "fr",
            true,
            DateTimeOffset.UtcNow,
            Actor).Value;

        Assert.True(notification.MarkRead(DateTimeOffset.UtcNow,Actor).IsSuccess);
        Assert.Equal(CommunicationNotificationStatus.Read,notification.Status);
    }
}
