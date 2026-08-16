using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Application.OrganizationConfigurations.Models;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationConfigurations.GetVersions;

public sealed record GetOrganizationConfigurationVersionsQuery(OrganizationId OrganizationId)
    : IQuery<IReadOnlyList<OrganizationConfigurationListItemResponse>>;
