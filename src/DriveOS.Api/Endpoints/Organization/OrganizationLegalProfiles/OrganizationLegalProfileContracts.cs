using DriveOS.Modules.Organizations.Domain.OrganizationLegalProfiles;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Api.Endpoints.Organization.OrganizationLegalProfiles;

public sealed record CreateOrganizationLegalProfileRequest(
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
    bool ActivateImmediately);

public sealed record UpdateOrganizationLegalProfileRequest(
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
    int ExpectedRevision);

public sealed record ChangeOrganizationLegalProfileStatusRequest(int ExpectedRevision);

public sealed record OrganizationLegalProfileResponseContract(
    Guid Id,
    Guid OrganizationId,
    string LegalForm,
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
    string Status,
    int Revision,
    DateTimeOffset CreatedAtUtc,
    Guid? CreatedByUserId,
    DateTimeOffset? LastModifiedAtUtc,
    Guid? LastModifiedByUserId);

internal sealed record CreateOrganizationLegalProfileApiModel(
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
    bool ActivateImmediately);

internal sealed record UpdateOrganizationLegalProfileApiModel(
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
    int ExpectedRevision);

internal sealed record ChangeOrganizationLegalProfileStatusApiModel(
    OrganizationId OrganizationId,
    int ExpectedRevision);
