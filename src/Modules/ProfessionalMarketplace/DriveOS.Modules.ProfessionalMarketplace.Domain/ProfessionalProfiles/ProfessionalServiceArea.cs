namespace DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;

/// <summary>
/// Searchable marketplace service area. Coordinates are deliberately approximate centroids;
/// a professional's private/residential address is never part of this marketplace object.
/// </summary>
public sealed record ProfessionalServiceArea(
    string AreaCode,
    string CountryCode,
    string DisplayName,
    decimal? Latitude,
    decimal? Longitude,
    int RadiusKm,
    bool Primary,
    ProfessionalMobilityMode MobilityMode);

public enum ProfessionalMobilityMode
{
    FixedArea = 1,
    Radius = 2,
    MultipleAreas = 3,
    Nationwide = 4
}
