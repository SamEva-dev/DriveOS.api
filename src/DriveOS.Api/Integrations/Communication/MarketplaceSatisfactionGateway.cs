using DriveOS.Modules.CommunicationEngagement.Application.Surveys;
using DriveOS.Modules.ProfessionalMarketplace.Application.Engagements;

namespace DriveOS.Api.Integrations.Communication;

internal sealed class MarketplaceSatisfactionGateway(
    ICommunicationSurveyRequestWriter surveys,
    ILogger<MarketplaceSatisfactionGateway> logger):IMarketplaceSatisfactionGateway
{
    public async Task<bool> TryRequestPartnerFeedbackAsync(
        MarketplaceSatisfactionRequest request,
        CancellationToken cancellationToken=default)
    {
        try
        {
            return await surveys.TryEnqueueAsync(new(
                request.RecipientUserId,
                request.OrganizationId,
                "PartnerFeedback",
                $"partner-feedback:first-paid:{request.EngagementId.Value}",
                "PROFESSIONAL_ENGAGEMENT",
                request.EngagementId.Value,
                new Dictionary<string,string?>
                {
                    ["engagementId"]=request.EngagementId.Value.ToString(),
                    ["firstPaidInvoiceId"]=request.FirstPaidInvoiceId.Value.ToString(),
                    ["paymentMethod"]=request.PaymentMethod,
                    ["cultureCode"]=request.CultureCode
                }),cancellationToken);
        }
        catch(Exception ex)
        {
            logger.LogError(ex,
                "Unable to enqueue partner feedback survey for engagement {EngagementId}.",
                request.EngagementId.Value);
            return false;
        }
    }
}
