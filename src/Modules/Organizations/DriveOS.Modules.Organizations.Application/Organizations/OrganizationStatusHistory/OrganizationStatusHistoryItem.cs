namespace DriveOS.Modules.Organizations.Application.Organizations.OrganizationStatusHistory;

public sealed record OrganizationStatusHistoryItem(
    Guid Id,
    string PreviousStatus,
    string NewStatus,
    string Reason,
    Guid ChangedByUserId,
    DateTimeOffset ChangedAtUtc);
