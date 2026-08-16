using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Domain.OrganizationRepresentatives.Events;

public sealed record OrganizationRepresentativeStatusChangedDomainEvent(
    OrganizationRepresentativeId RepresentativeId,
    OrganizationId OrganizationId,
    OrganizationRepresentativeStatus PreviousStatus,
    OrganizationRepresentativeStatus NewStatus,
    UserId ChangedByUserId,
    string Reason
) : DomainEvent;
