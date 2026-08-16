using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Application.OrganizationClosures.Models;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationClosures.GetByOrganization;

public sealed record GetOrganizationClosuresQuery(OrganizationId OrganizationId)
    : IQuery<IReadOnlyList<OrganizationClosureModel>>;
