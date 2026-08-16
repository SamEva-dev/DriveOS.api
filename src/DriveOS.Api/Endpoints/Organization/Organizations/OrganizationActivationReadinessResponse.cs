namespace DriveOS.Api.Endpoints.Organization.Organizations;

public sealed record OrganizationActivationReadinessResponse(
    Guid OrganizationId,
    bool IsReady,
    IReadOnlyCollection<OrganizationActivationRequirementResponse> Requirements,
    IReadOnlyCollection<OrganizationActivationRequirementResponse> BlockingRequirements
);

public sealed record OrganizationActivationRequirementResponse(
    string Code,
    bool IsSatisfied,
    string Severity,
    string MessageKey,
    IReadOnlyDictionary<string, object?> Parameters
);
