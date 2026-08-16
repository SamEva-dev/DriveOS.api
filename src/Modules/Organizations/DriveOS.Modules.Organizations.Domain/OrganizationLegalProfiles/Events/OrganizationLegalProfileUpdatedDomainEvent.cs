using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Domain.OrganizationLegalProfiles.Events;

public sealed record OrganizationLegalProfileUpdatedDomainEvent(
    OrganizationLegalProfileId LegalProfileId,
    OrganizationId OrganizationId,
    int Revision
) : DomainEvent;
