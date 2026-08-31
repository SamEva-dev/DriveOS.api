using System.Globalization;
using System.Net;
using Itech.Emailing.Abstractions;
using DriveOS.Modules.CommunicationEngagement.Application.Notifications;

namespace DriveOS.Api.Integrations.Communication;

internal sealed class ItechCommunicationNotificationEmailGateway(
    IEmailingService emailing,
    ILogger<ItechCommunicationNotificationEmailGateway> logger)
    :ICommunicationNotificationEmailGateway
{
    public async Task<Guid?> TryQueueAsync(
        CommunicationNotificationEmailRequest request,
        CancellationToken cancellationToken=default)
    {
        if(string.IsNullOrWhiteSpace(request.ToEmail))
            return null;

        try
        {
            bool fr=request.CultureCode.StartsWith("fr",StringComparison.OrdinalIgnoreCase);
            (string subject,string html)=Render(request.TemplateKey,request.Parameters,fr);
            string text=WebUtility.HtmlDecode(System.Text.RegularExpressions.Regex.Replace(html,"<[^>]+>"," "));
            return await emailing.QueueHtmlAsync(
                request.ToEmail.Trim(),subject,html,text,null,
                EmailUseCaseTags.NotificationSystem,cancellationToken);
        }
        catch(Exception ex)
        {
            logger.LogError(ex,"Unable to queue Communication notification email {TemplateKey}.",request.TemplateKey);
            return null;
        }
    }

    private static (string Subject,string Html) Render(
        string templateKey,
        IReadOnlyDictionary<string,string?> p,
        bool fr)
    {
        string V(string key)=>WebUtility.HtmlEncode(p.TryGetValue(key,out var value)?value??string.Empty:string.Empty);

        return templateKey switch
        {
            "professionalMarketplace.notifications.missionProposed" => fr
                ? ("Nouvelle mission DriveOS",
                   $"<p>Une nouvelle mission <strong>{V("title")}</strong> vous a été proposée.</p><p>Période : {V("startsOn")} → {V("endsOn")}.</p>")
                : ("New DriveOS mission",
                   $"<p>A new mission <strong>{V("title")}</strong> has been proposed to you.</p><p>Period: {V("startsOn")} → {V("endsOn")}.</p>"),

            "professionalMarketplace.notifications.serviceEntryRejected" => fr
                ? ("Prestation rejetée",$"<p>Une prestation a été rejetée.</p><p>Motif : {V("reason")}</p>")
                : ("Service entry rejected",$"<p>A service entry has been rejected.</p><p>Reason: {V("reason")}</p>"),

            "professionalMarketplace.notifications.serviceEntryDisputed" => fr
                ? ("Prestation contestée",$"<p>Une prestation a été contestée.</p><p>Motif : {V("reason")}</p>")
                : ("Service entry disputed",$"<p>A service entry has been disputed.</p><p>Reason: {V("reason")}</p>"),

            "professionalMarketplace.notifications.paymentReceived" => fr
                ? ("Paiement reçu",$"<p>Votre paiement de <strong>{V("amount")} {V("currency")}</strong> a été confirmé.</p>")
                : ("Payment received",$"<p>Your payment of <strong>{V("amount")} {V("currency")}</strong> has been confirmed.</p>"),

            "professionalMarketplace.notifications.paymentFailed" => fr
                ? ("Échec de paiement",$"<p>Une tentative de paiement de <strong>{V("amount")} {V("currency")}</strong> a échoué.</p><p>{V("reason")}</p>")
                : ("Payment failed",$"<p>A payment attempt of <strong>{V("amount")} {V("currency")}</strong> failed.</p><p>{V("reason")}</p>"),

            "professionalMarketplace.notifications.engagementCompleted" => fr
                ? ("Collaboration terminée","<p>Votre collaboration professionnelle DriveOS est maintenant terminée.</p>")
                : ("Engagement completed","<p>Your DriveOS professional engagement has now ended.</p>"),

            "professionalMarketplace.notifications.engagementTerminated" => fr
                ? ("Collaboration résiliée",$"<p>Votre collaboration professionnelle DriveOS a été résiliée.</p><p>Motif : {V("reason")}</p>")
                : ("Engagement terminated",$"<p>Your DriveOS professional engagement has been terminated.</p><p>Reason: {V("reason")}</p>"),

            "professionalMarketplace.notifications.compliancePolicyApplied" => fr
                ? ("Conformité professionnelle",$"<p>Une mesure de conformité a été appliquée : <strong>{V("action")}</strong>.</p><p>Exigence(s) : {V("requirements")}</p>")
                : ("Professional compliance",$"<p>A compliance measure has been applied: <strong>{V("action")}</strong>.</p><p>Requirement(s): {V("requirements")}</p>"),

            "professionalMarketplace.notifications.initialIntegrationCompleted" => fr
                ? ("Intégration freelance terminée",$"<p>Votre première facture a été payée. Votre intégration initiale est maintenant terminée.</p><p>Mode de paiement confirmé : {V("paymentMethod")}</p>")
                : ("Freelance onboarding completed",$"<p>Your first invoice has been paid. Your initial onboarding is now complete.</p><p>Confirmed payment method: {V("paymentMethod")}</p>"),

            _ => fr
                ? ("Notification DriveOS","<p>Une nouvelle notification est disponible dans votre espace DriveOS.</p>")
                : ("DriveOS notification","<p>A new notification is available in your DriveOS account.</p>")
        };
    }
}
