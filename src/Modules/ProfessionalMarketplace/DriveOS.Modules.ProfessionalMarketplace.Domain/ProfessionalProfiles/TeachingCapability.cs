namespace DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;

/// <summary>
/// Marketplace declaration of what the professional proposes to teach.
/// This is commercial/profile data, not proof of a regulatory authorization.
/// Regulatory eligibility remains verified by the compliance capability and
/// authoritative regulatory/workforce integrations.
/// </summary>
public sealed record TeachingCapability(
    string CategoryCode,
    string[] DeliveryModeCodes,
    string[] AudienceCodes,
    string[] LanguageCodes,
    string[] SpecializationCodes);
