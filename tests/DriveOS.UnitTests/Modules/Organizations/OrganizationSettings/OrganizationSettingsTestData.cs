using DriveOS.Modules.Organizations.Domain.OrganizationSettings;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.UnitTests.OrganizationSettings;

internal static class OrganizationSettingsTestData
{
    public static OrganizationProfile CreateProfile() =>
        Get(OrganizationProfile.Create(
            "Auto-école Horizon",
            "123456789",
            "FR123456789"));

    public static OrganizationContactInformation CreateContact() =>
        Get(OrganizationContactInformation.Create(
            "contact@horizon.test",
            "+33 4 00 00 00 00",
            "https://horizon.test"));

    public static OrganizationAddress CreateAddress() =>
        Get(OrganizationAddress.Create(
            "10 avenue de France",
            null,
            "06000",
            "Nice",
            "Provence-Alpes-Côte d'Azur",
            "FR"));

    public static OrganizationRegionalSettings CreateRegional() =>
        Get(OrganizationRegionalSettings.Create(
            "fr-FR",
            ["fr-FR", "en-GB"],
            "Europe/Paris",
            "EUR",
            "dd/MM/yyyy",
            "HH:mm",
            DayOfWeek.Monday,
            MeasurementSystem.Metric));

    public static OrganizationOperationalSettings CreateOperational(
        BranchId? defaultBranchId = null,
        bool requireBranch = false) =>
        Get(OrganizationOperationalSettings.Create(
            60,
            120,
            24,
            true,
            requireBranch,
            defaultBranchId));

    public static DriveOS.Modules.Organizations.Domain.OrganizationSettings.OrganizationSettings CreateAggregate() =>
        Get(DriveOS.Modules.Organizations.Domain.OrganizationSettings.OrganizationSettings.Create(
            OrganizationSettingsId.New(),
            OrganizationId.New(),
            CreateProfile(),
            CreateContact(),
            CreateAddress(),
            CreateRegional(),
            CreateOperational()));

    private static T Get<T>(Result<T> result)
    {
        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error.Code);
        }

        return result.Value;
    }
}
