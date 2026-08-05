using DriveOS.Modules.Organizations.Domain.OrganizationClosures;

namespace DriveOS.Api.Endpoints.OrganizationClosures;

public sealed record CreateOrganizationClosureRequest(
    OrganizationClosureReasonCode ReasonCode,
    string? ReasonDetails,
    DateTimeOffset RequestedEffectiveAtUtc,
    OrganizationDataDisposition DataDisposition,
    DateTimeOffset? RetentionUntilUtc);

public sealed record OrganizationClosureActionRequest(string? Comment);

public sealed record ScheduleOrganizationClosureRequest(DateTimeOffset ScheduledAtUtc);

public sealed record OrganizationClosureResponse(
    Guid Id,
    Guid OrganizationId,
    string ReasonCode,
    string? ReasonDetails,
    DateTimeOffset RequestedEffectiveAtUtc,
    string DataDisposition,
    DateTimeOffset? RetentionUntilUtc,
    string Status,
    int Revision,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ReviewedAtUtc,
    DateTimeOffset? ScheduledAtUtc,
    DateTimeOffset? CompletedAtUtc,
    DateTimeOffset? CancelledAtUtc,
    string? DecisionComment);

public sealed record OrganizationClosureRequirementResponse(
    string Code,
    bool IsSatisfied,
    string Severity,
    string MessageKey,
    IReadOnlyDictionary<string, object?> Parameters);

public sealed record OrganizationClosureReadinessResponse(
    Guid OrganizationId,
    bool CanClose,
    IReadOnlyList<OrganizationClosureRequirementResponse> Requirements,
    IReadOnlyList<OrganizationClosureRequirementResponse> BlockingRequirements);
