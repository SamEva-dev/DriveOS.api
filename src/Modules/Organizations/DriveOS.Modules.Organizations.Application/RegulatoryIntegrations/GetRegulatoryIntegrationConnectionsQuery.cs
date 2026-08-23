using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.RegulatoryIntegrations;

public sealed record GetRegulatoryIntegrationConnectionsQuery(OrganizationId OrganizationId)
    : IQuery<IReadOnlyList<RegulatoryIntegrationConnectionResponse>>;
