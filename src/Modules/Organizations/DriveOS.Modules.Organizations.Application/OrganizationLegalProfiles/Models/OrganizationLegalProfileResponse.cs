using DriveOS.Modules.Organizations.Domain.OrganizationLegalProfiles;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles.Models;

public sealed record OrganizationLegalProfileResponse(
    OrganizationLegalProfileId Id,
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
    OrganizationLegalProfileStatus Status,
    int Revision,
    DateTimeOffset CreatedAtUtc,
    UserId? CreatedByUserId,
    DateTimeOffset? LastModifiedAtUtc,
    UserId? LastModifiedByUserId);
