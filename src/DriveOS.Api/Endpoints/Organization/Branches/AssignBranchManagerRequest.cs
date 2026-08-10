namespace DriveOS.Api.Endpoints.Organization.Branches;

public sealed record AssignBranchManagerRequest(
    Guid ManagerUserId);