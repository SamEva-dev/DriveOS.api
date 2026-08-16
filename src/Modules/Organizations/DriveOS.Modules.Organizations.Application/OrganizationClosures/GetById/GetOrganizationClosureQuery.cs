using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Application.OrganizationClosures.Models;
using DriveOS.Modules.Organizations.Domain.OrganizationClosures;

namespace DriveOS.Modules.Organizations.Application.OrganizationClosures.GetById;

public sealed record GetOrganizationClosureQuery(OrganizationClosureId ClosureId)
    : IQuery<OrganizationClosureModel>;
