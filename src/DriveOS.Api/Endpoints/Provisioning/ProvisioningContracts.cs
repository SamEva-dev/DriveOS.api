namespace DriveOS.Api.Endpoints.Provisioning;

public sealed record ProvisionOrganizationRequest(
    Guid ExternalUserId,
    string LegalName,
    string CountryCode,
    int OrganizationType = 1);

public sealed record ProvisionOrganizationResponse(
    Guid OrganizationId,
    string Status);

public sealed record VerifyProvisionedOrganizationResponse(
    Guid Id,
    string Name,
    string Status,
    bool IsActive);
