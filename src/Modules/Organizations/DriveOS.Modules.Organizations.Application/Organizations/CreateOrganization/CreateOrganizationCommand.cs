using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.Organizations.CreateOrganization;

public sealed record CreateOrganizationCommand(
    string LegalName,
    string CountryCode,
    int OrganizationType
) : ICommand<OrganizationId>;
