namespace DriveOS.Api.Endpoints
    .BranchAssignments;

public sealed record
    BranchUserAssignmentResponse(
        Guid Id,
        Guid OrganizationId,
        Guid BranchId,
        Guid UserId,
        string Role,
        string AssignmentType,
        string Status,
        DateTimeOffset StartsAtUtc,
        DateTimeOffset? PlannedEndAtUtc,
        DateTimeOffset? EffectiveEndAtUtc,
        string? SuspensionReason,
        DateTimeOffset? SuspendedAtUtc,
        Guid? SuspendedByUserId,
        string? EndReason,
        DateTimeOffset? EndedAtUtc,
        Guid? EndedByUserId,
        DateTimeOffset CreatedAtUtc,
        Guid? CreatedByUserId,
        DateTimeOffset? LastModifiedAtUtc,
        Guid? LastModifiedByUserId);