using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.ProfessionalMarketplace.Domain.AccessGrants;
public static class ExternalAccessGrantErrors
{
    public static readonly Error NotFound=Error.NotFound("ProfessionalMarketplace.AccessGrants.NotFound","errors.professionalMarketplace.accessGrants.notFound");
    public static readonly Error InvalidIdentifier=Error.Validation("ProfessionalMarketplace.AccessGrants.InvalidIdentifier","errors.professionalMarketplace.accessGrants.invalidIdentifier");
    public static readonly Error InvalidGrant=Error.Validation("ProfessionalMarketplace.AccessGrants.InvalidGrant","errors.professionalMarketplace.accessGrants.invalidGrant");
    public static readonly Error OutsideEngagementPeriod=Error.Validation("ProfessionalMarketplace.AccessGrants.OutsideEngagementPeriod","errors.professionalMarketplace.accessGrants.outsideEngagementPeriod");
    public static readonly Error OutsideResourcePeriod=Error.Validation("ProfessionalMarketplace.AccessGrants.OutsideResourcePeriod","errors.professionalMarketplace.accessGrants.outsideResourcePeriod");
    public static readonly Error DuplicateGrant=Error.Conflict("ProfessionalMarketplace.AccessGrants.DuplicateGrant","errors.professionalMarketplace.accessGrants.duplicateGrant");
    public static readonly Error InvalidTransition=Error.Conflict("ProfessionalMarketplace.AccessGrants.InvalidTransition","errors.professionalMarketplace.accessGrants.invalidTransition");
    public static readonly Error RevocationReasonRequired=Error.Validation("ProfessionalMarketplace.AccessGrants.RevocationReasonRequired","errors.professionalMarketplace.accessGrants.revocationReasonRequired");
    public static readonly Error NotYetExpired=Error.Conflict("ProfessionalMarketplace.AccessGrants.NotYetExpired","errors.professionalMarketplace.accessGrants.notYetExpired");
    public static readonly Error ActiveEngagementRequired=Error.Conflict("ProfessionalMarketplace.AccessGrants.ActiveEngagementRequired","errors.professionalMarketplace.accessGrants.activeEngagementRequired");
    public static readonly Error AccessPreparationMustBeValidated=Error.Conflict("ProfessionalMarketplace.AccessGrants.AccessPreparationMustBeValidated","errors.professionalMarketplace.accessGrants.accessPreparationMustBeValidated");
}
