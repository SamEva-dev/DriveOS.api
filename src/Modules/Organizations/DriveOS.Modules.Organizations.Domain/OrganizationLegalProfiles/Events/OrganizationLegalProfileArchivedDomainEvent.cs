using DriveOS.Modules.Organizations.Domain.OrganizationLegalProfiles;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Domain.OrganizationLegalProfiles.Events;

public sealed record OrganizationLegalProfileArchivedDomainEvent(
    OrganizationLegalProfileId LegalProfileId,
    OrganizationId OrganizationId,
    int Revision
) : DomainEvent;
