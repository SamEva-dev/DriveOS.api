using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.ProfessionalMarketplace.Domain.Opportunities;
public static class ProfessionalOpportunityErrors
{
    public static readonly Error NotFound=Error.NotFound("ProfessionalMarketplace.Opportunities.NotFound","errors.professionalMarketplace.opportunities.notFound");
    public static readonly Error InvalidIdentifier=Error.Validation("ProfessionalMarketplace.Opportunities.InvalidIdentifier","errors.professionalMarketplace.opportunities.invalidIdentifier");
    public static readonly Error InvalidContent=Error.Validation("ProfessionalMarketplace.Opportunities.InvalidContent","errors.professionalMarketplace.opportunities.invalidContent");
    public static readonly Error InvalidRequirements=Error.Validation("ProfessionalMarketplace.Opportunities.InvalidRequirements","errors.professionalMarketplace.opportunities.invalidRequirements");
    public static readonly Error InvalidLocation=Error.Validation("ProfessionalMarketplace.Opportunities.InvalidLocation","errors.professionalMarketplace.opportunities.invalidLocation");
    public static readonly Error InvalidTimeWindows=Error.Validation("ProfessionalMarketplace.Opportunities.InvalidTimeWindows","errors.professionalMarketplace.opportunities.invalidTimeWindows");
    public static readonly Error InvalidBudget=Error.Validation("ProfessionalMarketplace.Opportunities.InvalidBudget","errors.professionalMarketplace.opportunities.invalidBudget");
    public static readonly Error InvalidTransition=Error.Conflict("ProfessionalMarketplace.Opportunities.InvalidTransition","errors.professionalMarketplace.opportunities.invalidTransition");
    public static readonly Error OpportunityExpired=Error.Conflict("ProfessionalMarketplace.Opportunities.OpportunityExpired","errors.professionalMarketplace.opportunities.opportunityExpired");
    public static readonly Error NotYetExpired=Error.Conflict("ProfessionalMarketplace.Opportunities.NotYetExpired","errors.professionalMarketplace.opportunities.notYetExpired");
    public static readonly Error ClosureReasonRequired=Error.Validation("ProfessionalMarketplace.Opportunities.ClosureReasonRequired","errors.professionalMarketplace.opportunities.closureReasonRequired");
}
