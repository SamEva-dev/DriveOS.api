using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.ProfessionalMarketplace.Domain.Compliance;

public static class ProfessionalComplianceWaiverErrors
{
    public static readonly Error NotFound=Error.NotFound("ProfessionalMarketplace.ComplianceWaivers.NotFound","errors.professionalMarketplace.complianceWaivers.notFound");
    public static readonly Error InvalidWaiver=Error.Validation("ProfessionalMarketplace.ComplianceWaivers.InvalidWaiver","errors.professionalMarketplace.complianceWaivers.invalidWaiver");
    public static readonly Error InvalidPeriod=Error.Validation("ProfessionalMarketplace.ComplianceWaivers.InvalidPeriod","errors.professionalMarketplace.complianceWaivers.invalidPeriod");
    public static readonly Error DuplicateWaiver=Error.Conflict("ProfessionalMarketplace.ComplianceWaivers.DuplicateWaiver","errors.professionalMarketplace.complianceWaivers.duplicateWaiver");
    public static readonly Error InvalidTransition=Error.Conflict("ProfessionalMarketplace.ComplianceWaivers.InvalidTransition","errors.professionalMarketplace.complianceWaivers.invalidTransition");
    public static readonly Error ReasonRequired=Error.Validation("ProfessionalMarketplace.ComplianceWaivers.ReasonRequired","errors.professionalMarketplace.complianceWaivers.reasonRequired");
    public static readonly Error NotYetExpired=Error.Conflict("ProfessionalMarketplace.ComplianceWaivers.NotYetExpired","errors.professionalMarketplace.complianceWaivers.notYetExpired");
}
