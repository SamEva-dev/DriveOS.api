using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Domain.OrganizationSettings;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationSettings.CreateOrganizationSettings;

public sealed record CreateOrganizationSettingsCommand(
    OrganizationId OrganizationId,
    string? TradeName,
    string? RegistrationNumber,
    string? TaxNumber,
    string? Email,
    string? Phone,
    string? Website,
    string? AddressLine1,
    string? AddressLine2,
    string? PostalCode,
    string? City,
    string? Region,
    string AddressCountryCode,
    string DefaultLanguage,
    IReadOnlyCollection<string> SupportedLanguages,
    string TimeZoneId,
    string CurrencyCode,
    string DateFormat,
    string TimeFormat,
    DayOfWeek FirstDayOfWeek,
    MeasurementSystem MeasurementSystem,
    int DefaultSessionDurationMinutes,
    int DefaultBookingLeadTimeMinutes,
    int DefaultCancellationDelayHours,
    bool AllowStudentSelfBooking,
    bool RequireBranchForOperations,
    BranchId? DefaultBranchId)
    : ICommand<OrganizationSettingsId>;
