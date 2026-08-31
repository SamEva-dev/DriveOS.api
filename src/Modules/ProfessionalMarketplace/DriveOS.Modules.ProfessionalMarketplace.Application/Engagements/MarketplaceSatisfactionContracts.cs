using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.Engagements;

public sealed record MarketplaceSatisfactionRequest(
    UserId RecipientUserId,
    OrganizationId OrganizationId,
    ProfessionalEngagementId EngagementId,
    ProfessionalInvoiceId FirstPaidInvoiceId,
    string? PaymentMethod,
    string? CultureCode);

public interface IMarketplaceSatisfactionGateway
{
    Task<bool> TryRequestPartnerFeedbackAsync(
        MarketplaceSatisfactionRequest request,
        CancellationToken cancellationToken=default);
}
