using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Domain.Networks.Events;

public sealed record NetworkOrganizationMemberAddedDomainEvent(
    NetworkOrganizationMembershipId MembershipId,
    OrganizationId NetworkOrganizationId,
    OrganizationId MemberOrganizationId,
    DateTimeOffset JoinedAtUtc
) : DomainEvent;

public sealed record NetworkOrganizationMemberRemovedDomainEvent(
    NetworkOrganizationMembershipId MembershipId,
    OrganizationId NetworkOrganizationId,
    OrganizationId MemberOrganizationId,
    DateTimeOffset EndedAtUtc
) : DomainEvent;
