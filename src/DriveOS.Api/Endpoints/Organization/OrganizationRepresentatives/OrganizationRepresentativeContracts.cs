using DriveOS.Modules.Organizations.Domain.OrganizationRepresentatives;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Api.Endpoints.Organization.OrganizationRepresentatives;

public sealed record CreateOrganizationRepresentativeRequest(
    Guid? PersonId,
    Guid? UserId,
    OrganizationRepresentativeType RepresentativeType,
    string AuthorityScope,
    bool IsPrimaryOwner,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    bool ActivateImmediately
);

public sealed record UpdateOrganizationRepresentativeAuthorityRequest(
    string AuthorityScope,
    Guid? UserId,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    int ExpectedRevision
);

public sealed record ChangeOrganizationRepresentativeStatusRequest(int ExpectedRevision);

public sealed record ChangeOrganizationRepresentativeStatusWithReasonRequest(
    string Reason,
    int ExpectedRevision
);

public sealed record EndOrganizationRepresentativeRequest(
    DateOnly EffectiveTo,
    string Reason,
    int ExpectedRevision
);

public sealed record OrganizationRepresentativeResponseContract(
    Guid Id,
    Guid OrganizationId,
    Guid PersonId,
    Guid? UserId,
    string RepresentativeType,
    string AuthorityScope,
    bool IsPrimaryOwner,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    string Status,
    int Revision,
    DateTimeOffset CreatedAtUtc,
    Guid? CreatedByUserId,
    DateTimeOffset? LastModifiedAtUtc,
    Guid? LastModifiedByUserId
);

public sealed record OrganizationRepresentativeListItemContract(
    Guid Id,
    Guid PersonId,
    Guid? UserId,
    string RepresentativeType,
    string AuthorityScope,
    bool IsPrimaryOwner,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    string Status,
    int Revision
);

internal sealed record CreateOrganizationRepresentativeApiModel(
    OrganizationId OrganizationId,
    PersonId PersonId,
    UserId? UserId,
    OrganizationRepresentativeType RepresentativeType,
    string AuthorityScope,
    bool IsPrimaryOwner,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    bool ActivateImmediately
);

internal sealed record UpdateOrganizationRepresentativeAuthorityApiModel(
    OrganizationId OrganizationId,
    OrganizationRepresentativeId RepresentativeId,
    string AuthorityScope,
    UserId? UserId,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    int ExpectedRevision
);

internal sealed record ChangeOrganizationRepresentativeStatusApiModel(
    OrganizationId OrganizationId,
    OrganizationRepresentativeId RepresentativeId,
    int ExpectedRevision
);

internal sealed record ChangeOrganizationRepresentativeStatusWithReasonApiModel(
    OrganizationId OrganizationId,
    OrganizationRepresentativeId RepresentativeId,
    string Reason,
    int ExpectedRevision
);

internal sealed record EndOrganizationRepresentativeApiModel(
    OrganizationId OrganizationId,
    OrganizationRepresentativeId RepresentativeId,
    DateOnly EffectiveTo,
    string Reason,
    int ExpectedRevision
);
