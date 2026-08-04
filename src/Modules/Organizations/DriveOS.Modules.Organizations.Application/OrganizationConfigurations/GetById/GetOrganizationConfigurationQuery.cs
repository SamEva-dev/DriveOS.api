using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Application.OrganizationConfigurations.Models;
using DriveOS.Modules.Organizations.Domain.OrganizationConfigurations;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationConfigurations.GetById;

public sealed record GetOrganizationConfigurationQuery(
    OrganizationId OrganizationId,
    OrganizationConfigurationId ConfigurationId)
    : IQuery<OrganizationConfigurationResponse>;
