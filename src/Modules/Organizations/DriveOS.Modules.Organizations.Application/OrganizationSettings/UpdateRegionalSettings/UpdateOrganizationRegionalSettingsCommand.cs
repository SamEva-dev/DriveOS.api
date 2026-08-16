using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Domain.OrganizationSettings;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationSettings.UpdateRegionalSettings;

public sealed record UpdateOrganizationRegionalSettingsCommand(
    OrganizationId OrganizationId,
    string DefaultLanguage,
    IReadOnlyCollection<string> SupportedLanguages,
    string TimeZoneId,
    string CurrencyCode,
    string DateFormat,
    string TimeFormat,
    DayOfWeek FirstDayOfWeek,
    MeasurementSystem MeasurementSystem,
    int ExpectedVersion
) : ICommand;
