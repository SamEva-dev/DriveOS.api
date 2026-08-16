namespace DriveOS.Modules.CRM.Domain.Offers;

public enum OfferDeliveryChannel
{
    Email = 1,
    SmsLink = 2,
    StudentPortal = 3,
    GuardianPortal = 4,
    Printed = 5,
    SecureLink = 6,
}

public enum OfferRecipientType
{
    Prospect = 1,
    LegalRepresentative = 2,
    Payer = 3,
    Company = 4,
    Funder = 5,
}

public enum OfferDeliveryStatus
{
    Preparing = 1,
    Ready = 2,
    Sending = 3,
    Sent = 4,
    PartiallySent = 5,
    DeliveryFailed = 6,
    Viewed = 7,
    LinkExpired = 8,
}

public static class OfferDeliveryChannelExtensions
{
    public static bool RequiresSecureLink(this OfferDeliveryChannel channel) =>
        channel
            is OfferDeliveryChannel.Email
                or OfferDeliveryChannel.SmsLink
                or OfferDeliveryChannel.StudentPortal
                or OfferDeliveryChannel.GuardianPortal
                or OfferDeliveryChannel.SecureLink;
}

public sealed record OfferRecipientDraft(
    OfferRecipientType Type,
    string DisplayName,
    string Address
);
