using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Domain.OrganizationRepresentatives.Events;

public sealed record OrganizationRepresentativeCreatedDomainEvent(
    OrganizationRepresentativeId RepresentativeId,
    OrganizationId OrganizationId,
    PersonId PersonId,
    OrganizationRepresentativeType RepresentativeType) : DomainEvent;
