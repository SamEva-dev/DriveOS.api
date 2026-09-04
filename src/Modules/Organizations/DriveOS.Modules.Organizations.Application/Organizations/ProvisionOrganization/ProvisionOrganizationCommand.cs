using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.Organizations.ProvisionOrganization;

public sealed record ProvisionOrganizationCommand(
    UserId ExternalUserId,
    string IdempotencyKey,
    string LegalName,
    string CountryCode,
    int OrganizationType
) : ICommand<ProvisionOrganizationResult>;

public sealed record ProvisionOrganizationResult(
    OrganizationId OrganizationId,
    string Status,
    bool WasCreated
);
