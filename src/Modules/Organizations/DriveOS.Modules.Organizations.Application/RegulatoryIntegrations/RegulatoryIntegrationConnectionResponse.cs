namespace DriveOS.Modules.Organizations.Application.RegulatoryIntegrations;

public sealed record RegulatoryIntegrationConnectionResponse(
    Guid Id,
    Guid OrganizationId,
    Guid? BranchId,
    string CountryCode,
    string ProviderCode,
    string ExternalAccountReference,
    bool HasSecretReference,
    string Status,
    int Revision);
