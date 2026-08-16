namespace DriveOS.Modules.Students.Domain.Students;

public sealed record StudentIdentityData(
    string LegalFirstName,
    string LegalLastName,
    string? PreferredName,
    DateOnly? BirthDate,
    string? BirthPlace,
    string? Nationality,
    string? Email,
    string? Phone,
    string? AddressLine1,
    string? AddressLine2,
    string? PostalCode,
    string? City,
    string? CountryCode,
    string? PreferredLanguage,
    string? TimeZone,
    bool AllowEmail = true,
    bool AllowSms = true,
    bool AllowPhone = true
);
