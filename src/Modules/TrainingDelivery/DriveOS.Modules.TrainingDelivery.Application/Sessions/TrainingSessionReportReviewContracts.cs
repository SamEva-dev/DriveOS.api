using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Application.Sessions;

public sealed record TrainingSessionReportReviewCheck(string Code, bool Passed, bool Blocking, string MessageKey);

public sealed record TrainingSessionReportReviewResponse(
    Guid SessionId, int ReportStatus, int ServerVersion, bool CanSubmit,
    IReadOnlyCollection<TrainingSessionReportReviewCheck> Checks);

public sealed record GetTrainingSessionReportReviewQuery(OrganizationId OrganizationId, TrainingSessionId SessionId)
    : IQuery<TrainingSessionReportReviewResponse>;

public sealed record MarkTrainingSessionReportReadyCommand(OrganizationId OrganizationId, TrainingSessionId SessionId, Guid OperationId, int ExpectedVersion, UserId ActorUserId)
    : ICommand<TrainingSessionResponse>;

public sealed record SubmitTrainingSessionReportCommand(OrganizationId OrganizationId, TrainingSessionId SessionId, Guid OperationId, int ExpectedVersion, bool RequestSupervisorReview, UserId ActorUserId)
    : ICommand<TrainingSessionResponse>;
