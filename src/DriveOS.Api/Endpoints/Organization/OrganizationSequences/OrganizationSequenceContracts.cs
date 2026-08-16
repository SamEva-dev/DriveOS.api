using DriveOS.Modules.Organizations.Domain.OrganizationSequences;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Api.Endpoints.Organization.OrganizationSequences;

public sealed record CreateOrganizationSequenceRequest(
    Guid? BranchId,
    OrganizationSequenceScope Scope,
    string Code,
    string Pattern,
    int Padding,
    long InitialValue,
    OrganizationSequenceResetPolicy ResetPolicy
);

public sealed record ReserveOrganizationSequenceNumberRequest(Guid? BranchId, string Code);

public sealed record ChangeOrganizationSequenceStatusRequest(int ExpectedRevision);

public sealed record OrganizationSequenceNumberResponse(string Value);

public sealed record OrganizationSequenceResponseContract(
    Guid Id,
    Guid OrganizationId,
    Guid? BranchId,
    string Scope,
    string Code,
    string Pattern,
    int Padding,
    long NextValue,
    string ResetPolicy,
    int? LastResetYear,
    int? LastResetMonth,
    string Status,
    int Revision,
    DateTimeOffset CreatedAtUtc,
    Guid? CreatedByUserId,
    DateTimeOffset? LastModifiedAtUtc,
    Guid? LastModifiedByUserId
);

public sealed record OrganizationSequenceListItemContract(
    Guid Id,
    Guid? BranchId,
    string Scope,
    string Code,
    string Pattern,
    int Padding,
    long NextValue,
    string ResetPolicy,
    string Status,
    int Revision
);

internal sealed record CreateOrganizationSequenceApiModel(
    OrganizationId OrganizationId,
    BranchId? BranchId,
    OrganizationSequenceScope Scope,
    string Code,
    string Pattern,
    int Padding,
    long InitialValue,
    OrganizationSequenceResetPolicy ResetPolicy
);

internal sealed record ReserveOrganizationSequenceNumberApiModel(
    OrganizationId OrganizationId,
    BranchId? BranchId,
    string Code
);

internal sealed record ChangeOrganizationSequenceStatusApiModel(
    OrganizationId OrganizationId,
    OrganizationSequenceId SequenceId,
    int ExpectedRevision
);
