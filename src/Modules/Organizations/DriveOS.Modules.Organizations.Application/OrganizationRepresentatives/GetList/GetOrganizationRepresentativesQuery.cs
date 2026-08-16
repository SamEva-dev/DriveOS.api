using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.Models;
using DriveOS.Modules.Organizations.Domain.OrganizationRepresentatives;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.GetList;

public sealed record GetOrganizationRepresentativesQuery(
    OrganizationId OrganizationId,
    OrganizationRepresentativeStatus? Status
) : IQuery<IReadOnlyCollection<OrganizationRepresentativeListItem>>;
