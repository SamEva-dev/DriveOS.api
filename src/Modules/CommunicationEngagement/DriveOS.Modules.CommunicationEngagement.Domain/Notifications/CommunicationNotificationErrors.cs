using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.CommunicationEngagement.Domain.Notifications;
public static class CommunicationNotificationErrors
{
    public static readonly Error NotFound=Error.NotFound("Communication.Notifications.NotFound","errors.communication.notifications.notFound");
    public static readonly Error InvalidIdentifier=Error.Validation("Communication.Notifications.InvalidIdentifier","errors.communication.notifications.invalidIdentifier");
    public static readonly Error InvalidContent=Error.Validation("Communication.Notifications.InvalidContent","errors.communication.notifications.invalidContent");
    public static readonly Error InvalidTransition=Error.Conflict("Communication.Notifications.InvalidTransition","errors.communication.notifications.invalidTransition");
}
