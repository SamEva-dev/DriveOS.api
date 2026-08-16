using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Domain.Organizations.Events;

public sealed record OrganizationCreatedDomainEvent(
    OrganizationId OrganizationId,
    string LegalName,
    string CountryCode,
    OrganizationType OrganizationType
) : DomainEvent;
