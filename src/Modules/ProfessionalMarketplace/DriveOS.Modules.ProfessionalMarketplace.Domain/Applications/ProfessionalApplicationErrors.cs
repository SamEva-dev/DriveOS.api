using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.ProfessionalMarketplace.Domain.Applications;
public static class ProfessionalApplicationErrors
{
    public static readonly Error NotFound=Error.NotFound("ProfessionalMarketplace.Applications.NotFound","errors.professionalMarketplace.applications.notFound");
    public static readonly Error InvalidIdentifier=Error.Validation("ProfessionalMarketplace.Applications.InvalidIdentifier","errors.professionalMarketplace.applications.invalidIdentifier");
    public static readonly Error DuplicateApplication=Error.Conflict("ProfessionalMarketplace.Applications.DuplicateApplication","errors.professionalMarketplace.applications.duplicateApplication");
    public static readonly Error OpportunityNotOpen=Error.Conflict("ProfessionalMarketplace.Applications.OpportunityNotOpen","errors.professionalMarketplace.applications.opportunityNotOpen");
    public static readonly Error ProfileNotEligible=Error.Conflict("ProfessionalMarketplace.Applications.ProfileNotEligible","errors.professionalMarketplace.applications.profileNotEligible");
    public static readonly Error InvalidMessage=Error.Validation("ProfessionalMarketplace.Applications.InvalidMessage","errors.professionalMarketplace.applications.invalidMessage");
    public static readonly Error InvalidRate=Error.Validation("ProfessionalMarketplace.Applications.InvalidRate","errors.professionalMarketplace.applications.invalidRate");
    public static readonly Error InvalidAvailability=Error.Validation("ProfessionalMarketplace.Applications.InvalidAvailability","errors.professionalMarketplace.applications.invalidAvailability");
    public static readonly Error InvalidTransition=Error.Conflict("ProfessionalMarketplace.Applications.InvalidTransition","errors.professionalMarketplace.applications.invalidTransition");
    public static readonly Error DecisionReasonRequired=Error.Validation("ProfessionalMarketplace.Applications.DecisionReasonRequired","errors.professionalMarketplace.applications.decisionReasonRequired");
}
