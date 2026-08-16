using DriveOS.Modules.Organizations.Domain.OrganizationLegalProfiles;

namespace DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles.Compliance;

public sealed record OrganizationLegalProfileComplianceInput(
    string CountryCode,
    OrganizationLegalForm LegalForm,
    string RegistrationNumber,
    string? TaxNumber,
    DateOnly? IncorporationDate,
    string AddressLine1,
    string PostalCode,
    string City
);
