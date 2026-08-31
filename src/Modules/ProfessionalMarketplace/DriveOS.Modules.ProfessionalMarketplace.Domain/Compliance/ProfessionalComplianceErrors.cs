using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ProfessionalMarketplace.Domain.Compliance;

public static class ProfessionalComplianceErrors
{
    public static readonly Error InvalidIdentifier = Error.Validation("ProfessionalMarketplace.Compliance.InvalidIdentifier","errors.professionalMarketplace.compliance.invalidIdentifier");
    public static readonly Error InvalidDocumentMetadata = Error.Validation("ProfessionalMarketplace.Compliance.InvalidDocumentMetadata","errors.professionalMarketplace.compliance.invalidDocumentMetadata");
    public static readonly Error InvalidCredentialMetadata = Error.Validation("ProfessionalMarketplace.Compliance.InvalidCredentialMetadata","errors.professionalMarketplace.compliance.invalidCredentialMetadata");
    public static readonly Error InvalidValidityPeriod = Error.Validation("ProfessionalMarketplace.Compliance.InvalidValidityPeriod","errors.professionalMarketplace.compliance.invalidValidityPeriod");
    public static readonly Error InvalidDocumentTransition = Error.Conflict("ProfessionalMarketplace.Compliance.InvalidDocumentTransition","errors.professionalMarketplace.compliance.invalidDocumentTransition");
    public static readonly Error InvalidCredentialTransition = Error.Conflict("ProfessionalMarketplace.Compliance.InvalidCredentialTransition","errors.professionalMarketplace.compliance.invalidCredentialTransition");
    public static readonly Error DocumentExpired = Error.Conflict("ProfessionalMarketplace.Compliance.DocumentExpired","errors.professionalMarketplace.compliance.documentExpired");
    public static readonly Error CredentialNotCurrentlyValid = Error.Conflict("ProfessionalMarketplace.Compliance.CredentialNotCurrentlyValid","errors.professionalMarketplace.compliance.credentialNotCurrentlyValid");
    public static readonly Error RejectionReasonRequired = Error.Validation("ProfessionalMarketplace.Compliance.RejectionReasonRequired","errors.professionalMarketplace.compliance.rejectionReasonRequired");
    public static readonly Error DocumentNotFound = Error.NotFound("ProfessionalMarketplace.Compliance.DocumentNotFound","errors.professionalMarketplace.compliance.documentNotFound");
    public static readonly Error CredentialNotFound = Error.NotFound("ProfessionalMarketplace.Compliance.CredentialNotFound","errors.professionalMarketplace.compliance.credentialNotFound");
    public static readonly Error InvalidRequirement = Error.Validation("ProfessionalMarketplace.Compliance.InvalidRequirement","errors.professionalMarketplace.compliance.invalidRequirement");
    public static readonly Error InvalidRequirementTransition = Error.Conflict("ProfessionalMarketplace.Compliance.InvalidRequirementTransition","errors.professionalMarketplace.compliance.invalidRequirementTransition");
    public static readonly Error DuplicateRequirementVersion = Error.Conflict("ProfessionalMarketplace.Compliance.DuplicateRequirementVersion","errors.professionalMarketplace.compliance.duplicateRequirementVersion");
    public static readonly Error ProfileNotFound = Error.NotFound("ProfessionalMarketplace.Compliance.ProfileNotFound","errors.professionalMarketplace.compliance.profileNotFound");
}
