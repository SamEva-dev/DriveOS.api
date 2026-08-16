using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Domain.OrganizationConfigurations;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationConfigurations.CreateDraft;

public sealed record CreateOrganizationConfigurationDraftCommand(
    OrganizationId OrganizationId,
    int VersionNumber,
    string CountryCode,
    string PayloadJson
) : ICommand<OrganizationConfigurationId>;
