namespace DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;

/// <summary>
/// Indicative/commercial marketplace rate. It is never the authoritative contractual price.
/// Accepted commercial terms will later be snapshotted by offers/contracts and consumed by finance.
/// </summary>
public sealed record ProfessionalRate(
    string RateCode,
    ProfessionalRateUnit Unit,
    decimal Amount,
    string Currency,
    string? TeachingCategoryCode,
    ProfessionalVehicleProvisionMode VehicleProvisionMode,
    decimal? MileageRate,
    decimal? MinimumBillableQuantity,
    bool Negotiable,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo);

public enum ProfessionalRateUnit
{
    Hour = 1,
    HalfDay = 2,
    Day = 3,
    Session = 4,
    Mission = 5
}

public enum ProfessionalVehicleProvisionMode
{
    NotApplicable = 0,
    ClientProvided = 1,
    ProfessionalProvided = 2,
    Either = 3
}
