using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Application.OrganizationClosures.Models;
using DriveOS.Modules.Organizations.Domain.OrganizationClosures;

namespace DriveOS.Modules.Organizations.Application.OrganizationClosures.GetReadiness;

public sealed record GetOrganizationClosureReadinessQuery(OrganizationClosureId ClosureId)
    : IQuery<OrganizationClosureReadinessModel>;
