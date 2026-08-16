namespace DriveOS.Api.Endpoints.Organization.Organizations;

/// <summary>
/// Requête de création d’une organisation DriveOS.
/// </summary>
/// <param name="LegalName">
/// Raison sociale officielle.
/// </param>
/// <param name="CountryCode">
/// Code pays ISO 3166-1 alpha-2, par exemple FR.
/// </param>
/// <param name="OrganizationType">
/// Type de structure organisationnelle.
/// </param>
public sealed record CreateOrganizationRequest(
    string LegalName,
    string CountryCode,
    int OrganizationType
);
