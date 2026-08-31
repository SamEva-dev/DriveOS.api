using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.ProfessionalMarketplace.Domain.CommercialOffers;
public static class ProfessionalCommercialOfferErrors
{
    public static readonly Error NotFound=Error.NotFound("ProfessionalMarketplace.CommercialOffers.NotFound","errors.professionalMarketplace.commercialOffers.notFound");
    public static readonly Error InvalidIdentifier=Error.Validation("ProfessionalMarketplace.CommercialOffers.InvalidIdentifier","errors.professionalMarketplace.commercialOffers.invalidIdentifier");
    public static readonly Error InvalidSource=Error.Validation("ProfessionalMarketplace.CommercialOffers.InvalidSource","errors.professionalMarketplace.commercialOffers.invalidSource");
    public static readonly Error InvalidTerms=Error.Validation("ProfessionalMarketplace.CommercialOffers.InvalidTerms","errors.professionalMarketplace.commercialOffers.invalidTerms");
    public static readonly Error InvalidTransition=Error.Conflict("ProfessionalMarketplace.CommercialOffers.InvalidTransition","errors.professionalMarketplace.commercialOffers.invalidTransition");
    public static readonly Error BilateralAcceptanceRequired=Error.Conflict("ProfessionalMarketplace.CommercialOffers.BilateralAcceptanceRequired","errors.professionalMarketplace.commercialOffers.bilateralAcceptanceRequired");
    public static readonly Error CancellationReasonRequired=Error.Validation("ProfessionalMarketplace.CommercialOffers.CancellationReasonRequired","errors.professionalMarketplace.commercialOffers.cancellationReasonRequired");
}
