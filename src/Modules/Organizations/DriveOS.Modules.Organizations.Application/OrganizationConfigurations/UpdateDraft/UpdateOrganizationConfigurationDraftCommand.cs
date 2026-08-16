using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Domain.OrganizationConfigurations;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationConfigurations.UpdateDraft;

public sealed record UpdateOrganizationConfigurationDraftCommand(
    OrganizationId OrganizationId,
    OrganizationConfigurationId ConfigurationId,
    string PayloadJson,
    int ExpectedRevision
) : ICommand;
