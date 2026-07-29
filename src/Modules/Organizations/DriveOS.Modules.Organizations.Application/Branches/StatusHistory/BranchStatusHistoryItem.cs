namespace DriveOS.Modules.Organizations.Application
    .Branches.StatusHistory;

public sealed record BranchStatusHistoryItem(
    Guid Id,
    string PreviousStatus,
    string NewStatus,
    string Reason,
    Guid ChangedByUserId,
    DateTimeOffset ChangedAtUtc);