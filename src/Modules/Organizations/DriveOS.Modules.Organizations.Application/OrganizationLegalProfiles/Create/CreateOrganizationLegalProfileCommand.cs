using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Domain.OrganizationLegalProfiles;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles.Create;

public sealed record CreateOrganizationLegalProfileCommand(
    OrganizationId OrganizationId,
    OrganizationLegalForm LegalForm,
    string RegistrationNumber,
    string? TaxNumber,
    string? TradeName,
    DateOnly? IncorporationDate,
    string AddressLine1,
    string? AddressLine2,
    string PostalCode,
    string City,
    string? Region,
    string CountryCode,
    bool ActivateImmediately
) : ICommand<OrganizationLegalProfileId>;
