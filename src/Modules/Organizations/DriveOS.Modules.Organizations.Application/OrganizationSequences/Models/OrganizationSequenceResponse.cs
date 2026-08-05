namespace DriveOS.Modules.Organizations.Application.OrganizationSequences.Models;

public sealed record OrganizationSequenceResponse(
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
    Guid? LastModifiedByUserId);
