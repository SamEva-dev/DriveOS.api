using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;

public static class ProfessionalProfileErrors
{
    public static readonly Error InvalidIdentifier = Error.Validation("ProfessionalMarketplace.Profile.InvalidIdentifier", "errors.professionalMarketplace.profile.invalidIdentifier");
    public static readonly Error InvalidPerson = Error.Validation("ProfessionalMarketplace.Profile.InvalidPerson", "errors.professionalMarketplace.profile.invalidPerson");
    public static readonly Error InvalidProviderOrganization = Error.Validation("ProfessionalMarketplace.Profile.InvalidProviderOrganization", "errors.professionalMarketplace.profile.invalidProviderOrganization");
    public static readonly Error InvalidBusinessIdentity = Error.Validation("ProfessionalMarketplace.Profile.InvalidBusinessIdentity", "errors.professionalMarketplace.profile.invalidBusinessIdentity");
    public static readonly Error InvalidPresentation = Error.Validation("ProfessionalMarketplace.Profile.InvalidPresentation", "errors.professionalMarketplace.profile.invalidPresentation");
    public static readonly Error InvalidLanguages = Error.Validation("ProfessionalMarketplace.Profile.InvalidLanguages", "errors.professionalMarketplace.profile.invalidLanguages");
    public static readonly Error InvalidTeachingCategories = Error.Validation("ProfessionalMarketplace.Profile.InvalidTeachingCategories", "errors.professionalMarketplace.profile.invalidTeachingCategories");
    public static readonly Error InvalidServiceArea = Error.Validation("ProfessionalMarketplace.Profile.InvalidServiceArea", "errors.professionalMarketplace.profile.invalidServiceArea");
    public static readonly Error InvalidVehicleInformation = Error.Validation("ProfessionalMarketplace.Profile.InvalidVehicleInformation", "errors.professionalMarketplace.profile.invalidVehicleInformation");
    public static readonly Error InvalidEngagementPreferences = Error.Validation("ProfessionalMarketplace.Profile.InvalidEngagementPreferences", "errors.professionalMarketplace.profile.invalidEngagementPreferences");
    public static readonly Error ProfileIncomplete = Error.Conflict("ProfessionalMarketplace.Profile.Incomplete", "errors.professionalMarketplace.profile.incomplete");
    public static readonly Error AlreadyActive = Error.Conflict("ProfessionalMarketplace.Profile.AlreadyActive", "errors.professionalMarketplace.profile.alreadyActive");
    public static readonly Error ComplianceRequired = Error.Conflict("ProfessionalMarketplace.Profile.ComplianceRequired", "errors.professionalMarketplace.profile.complianceRequired");
    public static readonly Error Archived = Error.Conflict("ProfessionalMarketplace.Profile.Archived", "errors.professionalMarketplace.profile.archived");
    public static readonly Error NotFound = Error.NotFound("ProfessionalMarketplace.Profile.NotFound", "errors.professionalMarketplace.profile.notFound");
    public static readonly Error InvalidTeachingCapabilities = Error.Validation("ProfessionalMarketplace.Profile.InvalidTeachingCapabilities", "errors.professionalMarketplace.profile.invalidTeachingCapabilities");
    public static readonly Error VerifiedProfileRequiredForVisibility = Error.Conflict("ProfessionalMarketplace.Profile.VerifiedProfileRequiredForVisibility","errors.professionalMarketplace.profile.verifiedProfileRequiredForVisibility");
    public static readonly Error InvalidServiceAreas = Error.Validation("ProfessionalMarketplace.Profile.InvalidServiceAreas","errors.professionalMarketplace.profile.invalidServiceAreas");
    public static readonly Error InvalidAvailabilityPolicy = Error.Validation("ProfessionalMarketplace.Profile.InvalidAvailabilityPolicy","errors.professionalMarketplace.profile.invalidAvailabilityPolicy");
    public static readonly Error OverlappingAvailabilityRules = Error.Validation("ProfessionalMarketplace.Profile.OverlappingAvailabilityRules","errors.professionalMarketplace.profile.overlappingAvailabilityRules");
    public static readonly Error InvalidProfessionalRates = Error.Validation("ProfessionalMarketplace.Profile.InvalidProfessionalRates","errors.professionalMarketplace.profile.invalidProfessionalRates");
    public static readonly Error OverlappingProfessionalRates = Error.Validation("ProfessionalMarketplace.Profile.OverlappingProfessionalRates","errors.professionalMarketplace.profile.overlappingProfessionalRates");
}
