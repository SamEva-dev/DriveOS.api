using Itech.Emailing.Abstractions;
using DriveOS.Modules.ProfessionalMarketplace.Application.Invitations;

namespace DriveOS.Api.Integrations.ProfessionalMarketplace;

internal sealed class FreelanceInvitationDeliveryGateway(
    IEmailingService emailing,
    ILogger<FreelanceInvitationDeliveryGateway> logger):IFreelanceInvitationDeliveryGateway
{
    public async Task TrySendAsync(FreelanceInvitationDeliveryRequest request,CancellationToken ct=default)
    {
        if(string.IsNullOrWhiteSpace(request.Email))return;
        try
        {
            string subject="Invitation DriveOS — Collaboration professionnelle";
            string html=$"<p>Vous avez reçu une invitation à collaborer via DriveOS.</p><p>{System.Net.WebUtility.HtmlEncode(request.Message)}</p><p><a href=\"{System.Net.WebUtility.HtmlEncode(request.SecureUrl)}\">Ouvrir l’invitation</a></p><p>Valable jusqu’au {request.ExpirationDate:dd/MM/yyyy}.</p>";
            await emailing.QueueHtmlAsync(request.Email.Trim(),subject,html,null,null,EmailUseCaseTags.NotificationSystem,ct);
        }
        catch(Exception ex)
        {
            logger.LogError(ex,"Unable to send freelance invitation {InvitationId}.",request.InvitationId);
        }
    }
}
