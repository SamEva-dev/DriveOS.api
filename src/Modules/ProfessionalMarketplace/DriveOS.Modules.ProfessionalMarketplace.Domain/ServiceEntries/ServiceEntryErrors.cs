using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.ProfessionalMarketplace.Domain.ServiceEntries;
public static class ServiceEntryErrors
{
    public static readonly Error NotFound=Error.NotFound("ProfessionalMarketplace.ServiceEntries.NotFound","errors.professionalMarketplace.serviceEntries.notFound");
    public static readonly Error InvalidIdentifier=Error.Validation("ProfessionalMarketplace.ServiceEntries.InvalidIdentifier","errors.professionalMarketplace.serviceEntries.invalidIdentifier");
    public static readonly Error InvalidService=Error.Validation("ProfessionalMarketplace.ServiceEntries.InvalidService","errors.professionalMarketplace.serviceEntries.invalidService");
    public static readonly Error OutsideEngagementPeriod=Error.Validation("ProfessionalMarketplace.ServiceEntries.OutsideEngagementPeriod","errors.professionalMarketplace.serviceEntries.outsideEngagementPeriod");
    public static readonly Error DuplicateSource=Error.Conflict("ProfessionalMarketplace.ServiceEntries.DuplicateSource","errors.professionalMarketplace.serviceEntries.duplicateSource");
    public static readonly Error InvalidTransition=Error.Conflict("ProfessionalMarketplace.ServiceEntries.InvalidTransition","errors.professionalMarketplace.serviceEntries.invalidTransition");
    public static readonly Error ReasonRequired=Error.Validation("ProfessionalMarketplace.ServiceEntries.ReasonRequired","errors.professionalMarketplace.serviceEntries.reasonRequired");
    public static readonly Error ActiveEngagementRequired=Error.Conflict("ProfessionalMarketplace.ServiceEntries.ActiveEngagementRequired","errors.professionalMarketplace.serviceEntries.activeEngagementRequired");
}
