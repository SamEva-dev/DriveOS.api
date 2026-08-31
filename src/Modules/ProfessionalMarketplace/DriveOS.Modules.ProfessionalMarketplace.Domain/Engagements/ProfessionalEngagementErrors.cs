using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.ProfessionalMarketplace.Domain.Engagements;
public static class ProfessionalEngagementErrors
{
    public static readonly Error NotFound=Error.NotFound("ProfessionalMarketplace.Engagements.NotFound","errors.professionalMarketplace.engagements.notFound");
    public static readonly Error InvalidIdentifier=Error.Validation("ProfessionalMarketplace.Engagements.InvalidIdentifier","errors.professionalMarketplace.engagements.invalidIdentifier");
    public static readonly Error FinalizedOfferRequired=Error.Conflict("ProfessionalMarketplace.Engagements.FinalizedOfferRequired","errors.professionalMarketplace.engagements.finalizedOfferRequired");
    public static readonly Error DuplicateEngagement=Error.Conflict("ProfessionalMarketplace.Engagements.DuplicateEngagement","errors.professionalMarketplace.engagements.duplicateEngagement");
    public static readonly Error InvalidTransition=Error.Conflict("ProfessionalMarketplace.Engagements.InvalidTransition","errors.professionalMarketplace.engagements.invalidTransition");
    public static readonly Error InvalidPreparationStep=Error.Validation("ProfessionalMarketplace.Engagements.InvalidPreparationStep","errors.professionalMarketplace.engagements.invalidPreparationStep");
    public static readonly Error PreparationIncomplete=Error.Conflict("ProfessionalMarketplace.Engagements.PreparationIncomplete","errors.professionalMarketplace.engagements.preparationIncomplete");
    public static readonly Error OutsideEngagementPeriod=Error.Conflict("ProfessionalMarketplace.Engagements.OutsideEngagementPeriod","errors.professionalMarketplace.engagements.outsideEngagementPeriod");
    public static readonly Error StatusReasonRequired=Error.Validation("ProfessionalMarketplace.Engagements.StatusReasonRequired","errors.professionalMarketplace.engagements.statusReasonRequired");
    public static readonly Error EngagementNotEndedYet=Error.Conflict("ProfessionalMarketplace.Engagements.EngagementNotEndedYet","errors.professionalMarketplace.engagements.engagementNotEndedYet");
    public static readonly Error ProfessionalUserRequired=Error.Conflict("ProfessionalMarketplace.Engagements.ProfessionalUserRequired","errors.professionalMarketplace.engagements.professionalUserRequired");
    public static readonly Error SchedulingCategoryMismatch=Error.Conflict("ProfessionalMarketplace.Engagements.SchedulingCategoryMismatch","errors.professionalMarketplace.engagements.schedulingCategoryMismatch");
    public static readonly Error SchedulingTimeZoneRequired=Error.Conflict("ProfessionalMarketplace.Engagements.SchedulingTimeZoneRequired","errors.professionalMarketplace.engagements.schedulingTimeZoneRequired");
    public static readonly Error SchedulingPreparationMustBeValidated=Error.Conflict("ProfessionalMarketplace.Engagements.SchedulingPreparationMustBeValidated","errors.professionalMarketplace.engagements.schedulingPreparationMustBeValidated");
    public static readonly Error ContractPreparationMustBeValidated=Error.Conflict("ProfessionalMarketplace.Engagements.ContractPreparationMustBeValidated","errors.professionalMarketplace.engagements.contractPreparationMustBeValidated");
    public static readonly Error CompliancePreparationMustBeValidated=Error.Conflict("ProfessionalMarketplace.Engagements.CompliancePreparationMustBeValidated","errors.professionalMarketplace.engagements.compliancePreparationMustBeValidated");
    public static readonly Error SignedProfessionalContractRequired=Error.Conflict("ProfessionalMarketplace.Engagements.SignedProfessionalContractRequired","errors.professionalMarketplace.engagements.signedProfessionalContractRequired");
    public static readonly Error CompliantActiveProfessionalRequired=Error.Conflict("ProfessionalMarketplace.Engagements.CompliantActiveProfessionalRequired","errors.professionalMarketplace.engagements.compliantActiveProfessionalRequired");
}
