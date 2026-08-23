namespace DriveOS.Modules.Organizations.Application.RegulatoryIntegrations;

/// <summary>
/// Internal transport-facing view of an active regulatory integration connection.
/// SecretReference is a pointer only; secret material is never stored in Organizations.
/// This type must not be exposed by public HTTP endpoints.
/// </summary>
public sealed record RegulatoryIntegrationTransportConnectionSnapshot(
    Guid Id,
    Guid OrganizationId,
    Guid? BranchId,
    string CountryCode,
    string ProviderCode,
    string ExternalAccountReference,
    string? SecretReference,
    int Revision);
