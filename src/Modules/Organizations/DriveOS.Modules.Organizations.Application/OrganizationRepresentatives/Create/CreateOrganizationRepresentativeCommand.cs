using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Domain.OrganizationRepresentatives;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.Create;

public sealed record CreateOrganizationRepresentativeCommand(
    OrganizationId OrganizationId,
    PersonId PersonId,
    UserId? UserId,
    OrganizationRepresentativeType RepresentativeType,
    string AuthorityScope,
    bool IsPrimaryOwner,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    bool ActivateImmediately
) : ICommand<OrganizationRepresentativeId>;
