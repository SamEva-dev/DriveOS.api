using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Domain.Leads;

public sealed record RequestedTraining
{
    private RequestedTraining(
        string licenseCategory,
        TransmissionPreference transmission,
        string? preferredLocation
    )
    {
        LicenseCategory = licenseCategory;
        Transmission = transmission;
        PreferredLocation = preferredLocation;
    }

    private RequestedTraining() { }

    public string LicenseCategory { get; private init; } = string.Empty;

    public TransmissionPreference Transmission { get; private init; }

    public string? PreferredLocation { get; private init; }

    public static Result<RequestedTraining> Create(
        string licenseCategory,
        TransmissionPreference transmission,
        string? preferredLocation
    )
    {
        string normalizedCategory = licenseCategory?.Trim().ToUpperInvariant() ?? string.Empty;

        string? normalizedLocation = NormalizeOptional(preferredLocation);

        if (string.IsNullOrWhiteSpace(normalizedCategory))
        {
            return Result.Failure<RequestedTraining>(LeadErrors.LicenseCategoryRequired);
        }

        if (normalizedCategory.Length > 30)
        {
            return Result.Failure<RequestedTraining>(LeadErrors.LicenseCategoryTooLong);
        }

        if (!Enum.IsDefined(transmission))
        {
            return Result.Failure<RequestedTraining>(LeadErrors.InvalidTransmissionPreference);
        }

        if (normalizedLocation?.Length > 200)
        {
            return Result.Failure<RequestedTraining>(LeadErrors.PreferredLocationTooLong);
        }

        return Result.Success(
            new RequestedTraining(normalizedCategory, transmission, normalizedLocation)
        );
    }

    private static string? NormalizeOptional(string? value)
    {
        string normalized = value?.Trim() ?? string.Empty;
        return normalized.Length == 0 ? null : normalized;
    }
}
