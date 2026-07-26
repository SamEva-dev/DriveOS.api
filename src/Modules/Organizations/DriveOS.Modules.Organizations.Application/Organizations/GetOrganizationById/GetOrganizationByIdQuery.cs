using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application
    .Organizations.GetOrganizationById;

public sealed record GetOrganizationByIdQuery(
    OrganizationId OrganizationId)
    : IQuery<OrganizationResponse>;