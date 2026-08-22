using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Application.Places.Watch;

public sealed record CreateExamPlaceWatchCommand(
    OrganizationId OrganizationId,
    string ProviderCode,
    string CountryCode,
    string? AdministrativeAreaCode,
    string? ExamCategory,
    DateTimeOffset WindowFromUtc,
    DateTimeOffset WindowToUtc,
    int CheckIntervalMinutes,
    IReadOnlyCollection<string>? CenterExternalIds,
    UserId ActorUserId) : ICommand<ExamPlaceWatchSubscriptionId>;

public sealed record GetExamPlaceWatchesQuery(OrganizationId OrganizationId) : IQuery<IReadOnlyList<ExamPlaceWatchResponse>>;
public sealed record GetExamPlaceWatchScansQuery(OrganizationId OrganizationId, ExamPlaceWatchSubscriptionId SubscriptionId, int Take = 50) : IQuery<IReadOnlyList<ExamPlaceWatchScanResponse>>;

public sealed record PauseExamPlaceWatchCommand(OrganizationId OrganizationId, ExamPlaceWatchSubscriptionId SubscriptionId, UserId ActorUserId) : ICommand;
public sealed record ResumeExamPlaceWatchCommand(OrganizationId OrganizationId, ExamPlaceWatchSubscriptionId SubscriptionId, UserId ActorUserId) : ICommand;
public sealed record RunExamPlaceWatchCommand(OrganizationId OrganizationId, ExamPlaceWatchSubscriptionId SubscriptionId, UserId ActorUserId) : ICommand<ExamPlaceWatchRunResponse>;

public sealed record ExamPlaceWatchResponse(
    Guid Id,
    string ProviderCode,
    string CountryCode,
    string? AdministrativeAreaCode,
    string? ExamCategory,
    DateTimeOffset WindowFromUtc,
    DateTimeOffset WindowToUtc,
    int CheckIntervalMinutes,
    IReadOnlyCollection<string> CenterExternalIds,
    string Status,
    DateTimeOffset NextCheckAtUtc,
    DateTimeOffset? LastCheckedAtUtc,
    DateTimeOffset? LastSuccessfulCheckAtUtc,
    DateTimeOffset? LastAvailabilityDetectedAtUtc,
    string? LastErrorCode,
    int ConsecutiveFailureCount);

public sealed record ExamPlaceWatchRunResponse(
    Guid SubscriptionId,
    bool Success,
    DateTimeOffset CheckedAtUtc,
    int ExternalSlotsRead,
    int NewAvailabilitiesDetected,
    string? ErrorCode);

public sealed record ExamPlaceWatchScanResponse(Guid Id, DateTimeOffset StartedAtUtc, DateTimeOffset? CompletedAtUtc, bool IsSuccess, int ExternalSlotsRead, int NewAvailabilitiesDetected, string? ErrorCode);
