namespace DriveOS.Api.Endpoints.Branches;

public sealed record BranchManagerAssignmentResponse(
    Guid Id,
    Guid BranchId,
    Guid ManagerUserId,
    DateTimeOffset EffectiveFromUtc,
    DateTimeOffset? EffectiveToUtc,
    string Status,
    Guid AssignedByUserId,
    DateTimeOffset AssignedAtUtc,
    Guid? EndedByUserId,
    DateTimeOffset? EndedAtUtc);