using DriveOS.Modules.Organizations.Domain.OrganizationRepresentatives;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.AccessSynchronization;

public sealed record OrganizationRepresentativeAccessSnapshot(
    OrganizationRepresentativeId RepresentativeId,
    OrganizationId OrganizationId,
    PersonId PersonId,
    UserId UserId,
    OrganizationRepresentativeType RepresentativeType,
    string AuthorityScope,
    bool IsPrimaryOwner,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    OrganizationRepresentativeStatus Status,
    int Revision
);
