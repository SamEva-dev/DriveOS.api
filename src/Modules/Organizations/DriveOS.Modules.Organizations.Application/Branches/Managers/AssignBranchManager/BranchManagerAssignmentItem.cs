namespace DriveOS.Modules.Organizations.Application
    .Branches.Managers;

public sealed record BranchManagerAssignmentItem(
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