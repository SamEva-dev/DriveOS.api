using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Domain.OrganizationConfigurations;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationConfigurations.Archive;

public sealed record ArchiveOrganizationConfigurationCommand(
    OrganizationId OrganizationId,
    OrganizationConfigurationId ConfigurationId,
    int ExpectedRevision
) : ICommand;
