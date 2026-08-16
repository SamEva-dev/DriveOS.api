using DriveOS.Modules.Organizations.Domain.OrganizationSettings;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Api.Endpoints.Organization.OrganizationSettings;

public sealed record CreateOrganizationSettingsRequest(
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
    Guid? DefaultBranchId
);

public sealed record UpdateOrganizationProfileRequest(
    string? TradeName,
    string? RegistrationNumber,
    string? TaxNumber,
    int ExpectedVersion
);

public sealed record UpdateOrganizationContactRequest(
    string? Email,
    string? Phone,
    string? Website,
    int ExpectedVersion
);

public sealed record UpdateOrganizationAddressRequest(
    string? AddressLine1,
    string? AddressLine2,
    string? PostalCode,
    string? City,
    string? Region,
    string AddressCountryCode,
    int ExpectedVersion
);

public sealed record UpdateOrganizationRegionalSettingsRequest(
    string DefaultLanguage,
    IReadOnlyCollection<string> SupportedLanguages,
    string TimeZoneId,
    string CurrencyCode,
    string DateFormat,
    string TimeFormat,
    DayOfWeek FirstDayOfWeek,
    MeasurementSystem MeasurementSystem,
    int ExpectedVersion
);

public sealed record UpdateOrganizationOperationalSettingsRequest(
    int DefaultSessionDurationMinutes,
    int DefaultBookingLeadTimeMinutes,
    int DefaultCancellationDelayHours,
    bool AllowStudentSelfBooking,
    bool RequireBranchForOperations,
    Guid? DefaultBranchId,
    int ExpectedVersion
);

public sealed record OrganizationSettingsResponseContract(
    Guid Id,
    Guid OrganizationId,
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
    int FirstDayOfWeek,
    int MeasurementSystem,
    int DefaultSessionDurationMinutes,
    int DefaultBookingLeadTimeMinutes,
    int DefaultCancellationDelayHours,
    bool AllowStudentSelfBooking,
    bool RequireBranchForOperations,
    Guid? DefaultBranchId,
    int Version,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? LastModifiedAtUtc
);

internal sealed record CreateOrganizationSettingsApiModel(
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
    BranchId? DefaultBranchId
);

internal sealed record UpdateOrganizationProfileApiModel(
    OrganizationId OrganizationId,
    string? TradeName,
    string? RegistrationNumber,
    string? TaxNumber,
    int ExpectedVersion
);

internal sealed record UpdateOrganizationContactApiModel(
    OrganizationId OrganizationId,
    string? Email,
    string? Phone,
    string? Website,
    int ExpectedVersion
);

internal sealed record UpdateOrganizationAddressApiModel(
    OrganizationId OrganizationId,
    string? AddressLine1,
    string? AddressLine2,
    string? PostalCode,
    string? City,
    string? Region,
    string AddressCountryCode,
    int ExpectedVersion
);

internal sealed record UpdateOrganizationRegionalSettingsApiModel(
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
);

internal sealed record UpdateOrganizationOperationalSettingsApiModel(
    OrganizationId OrganizationId,
    int DefaultSessionDurationMinutes,
    int DefaultBookingLeadTimeMinutes,
    int DefaultCancellationDelayHours,
    bool AllowStudentSelfBooking,
    bool RequireBranchForOperations,
    BranchId? DefaultBranchId,
    int ExpectedVersion
);
