using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.Models;
using DriveOS.Modules.Organizations.Domain.OrganizationRepresentatives;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.GetById;

public sealed record GetOrganizationRepresentativeByIdQuery(
    OrganizationId OrganizationId,
    OrganizationRepresentativeId RepresentativeId
) : IQuery<OrganizationRepresentativeResponse>;
