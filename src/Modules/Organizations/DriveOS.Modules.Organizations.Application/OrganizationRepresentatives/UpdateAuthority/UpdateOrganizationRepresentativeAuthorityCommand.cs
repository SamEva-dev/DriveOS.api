using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Domain.OrganizationRepresentatives;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.UpdateAuthority;

public sealed record UpdateOrganizationRepresentativeAuthorityCommand(
    OrganizationId OrganizationId,
    OrganizationRepresentativeId RepresentativeId,
    string AuthorityScope,
    UserId? UserId,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    int ExpectedRevision
) : ICommand;
