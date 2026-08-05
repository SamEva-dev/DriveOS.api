using DriveOS.Modules.Organizations.Domain.OrganizationRepresentatives;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.Models;

public sealed record OrganizationRepresentativeListItem(
    OrganizationRepresentativeId Id,
    PersonId PersonId,
    UserId? UserId,
    OrganizationRepresentativeType RepresentativeType,
    string AuthorityScope,
    bool IsPrimaryOwner,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    OrganizationRepresentativeStatus Status,
    int Revision);
