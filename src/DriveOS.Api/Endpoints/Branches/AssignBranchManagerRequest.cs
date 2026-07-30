namespace DriveOS.Api.Endpoints.Branches;

public sealed record AssignBranchManagerRequest(
    Guid ManagerUserId,
    DateTimeOffset? EffectiveFromUtc);