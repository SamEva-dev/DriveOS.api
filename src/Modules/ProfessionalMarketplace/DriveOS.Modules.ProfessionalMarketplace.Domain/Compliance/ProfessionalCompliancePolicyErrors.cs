using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.ProfessionalMarketplace.Domain.Compliance;

public static class ProfessionalCompliancePolicyErrors
{
    public static readonly Error NotFound=Error.NotFound("ProfessionalMarketplace.CompliancePolicies.NotFound","errors.professionalMarketplace.compliancePolicies.notFound");
    public static readonly Error InvalidPolicy=Error.Validation("ProfessionalMarketplace.CompliancePolicies.InvalidPolicy","errors.professionalMarketplace.compliancePolicies.invalidPolicy");
    public static readonly Error InvalidPeriod=Error.Validation("ProfessionalMarketplace.CompliancePolicies.InvalidPeriod","errors.professionalMarketplace.compliancePolicies.invalidPeriod");
    public static readonly Error DuplicatePolicy=Error.Conflict("ProfessionalMarketplace.CompliancePolicies.DuplicatePolicy","errors.professionalMarketplace.compliancePolicies.duplicatePolicy");
    public static readonly Error InvalidTransition=Error.Conflict("ProfessionalMarketplace.CompliancePolicies.InvalidTransition","errors.professionalMarketplace.compliancePolicies.invalidTransition");
}
