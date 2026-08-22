using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Application.Remediation;

public sealed record ExamRemediationRequestResponse(Guid Id, Guid FailureAnalysisId, Guid ExamResultId, int ResultRevision,
    Guid FailedAttemptId, Guid RegistrationId, Guid StudentId, int FailedAttemptNumber, Guid? TrainingPathId,
    string AnalysisSummary, string? RecommendationSummary, IReadOnlyCollection<Guid> AffectedCompetencyIds,
    IReadOnlyCollection<string> RecommendationCodes, int? RecommendedHours, Guid? ResponsibleUserId, DateOnly? ReviewDate,
    DateOnly? TargetDate, bool MockExamRequired, bool FundingReviewRequired, Guid? PedagogicalRemediationPlanId, string Status, string? DeferredReasonCode, string? FailureCode,
    DateTimeOffset? ProvisionedAtUtc, DateTimeOffset? CompletedAtUtc, DateTimeOffset? ValidatedForRePresentationAtUtc,
    Guid? ValidatedByUserId, DateTimeOffset? SupersededAtUtc);

public sealed record CreateExamRemediationRequestCommand(OrganizationId OrganizationId, ExamResultId ResultId, int ResultRevision,
    UserId ActorUserId) : ICommand<ExamRemediationRequestResponse>;
public sealed record ConfigureExamRemediationRequestCommand(OrganizationId OrganizationId, ExamRemediationRequestId RequestId,
    TrainingPathId TrainingPathId, UserId ResponsibleUserId, DateOnly ReviewDate, DateOnly? TargetDate, bool MockExamRequired,
    bool FundingReviewRequired, int? RecommendedHours, UserId ActorUserId)
    : ICommand<ExamRemediationRequestResponse>;
public sealed record ProvisionExamRemediationPlanCommand(OrganizationId OrganizationId, ExamRemediationRequestId RequestId,
    UserId ActorUserId) : ICommand<ExamRemediationRequestResponse>;
public sealed record RefreshExamRemediationRequestCommand(OrganizationId OrganizationId, ExamRemediationRequestId RequestId,
    UserId ActorUserId) : ICommand<ExamRemediationRequestResponse>;
public sealed record ValidateExamRemediationForRePresentationCommand(OrganizationId OrganizationId, ExamRemediationRequestId RequestId,
    UserId ActorUserId) : ICommand<ExamRemediationRequestResponse>;
public sealed record CancelExamRemediationRequestCommand(OrganizationId OrganizationId, ExamRemediationRequestId RequestId,
    string Reason, UserId ActorUserId) : ICommand<ExamRemediationRequestResponse>;
public sealed record GetExamRemediationRequestQuery(OrganizationId OrganizationId, ExamRemediationRequestId RequestId)
    : IQuery<ExamRemediationRequestResponse>;
public sealed record GetExamRemediationByResultQuery(OrganizationId OrganizationId, ExamResultId ResultId, int ResultRevision)
    : IQuery<ExamRemediationRequestResponse>;
public sealed record GetStudentExamRemediationsQuery(OrganizationId OrganizationId, PersonId StudentId)
    : IQuery<IReadOnlyList<ExamRemediationRequestResponse>>;

public sealed record ExamRemediationProvisionRequest(OrganizationId OrganizationId, TrainingPathId TrainingPathId,
    UserId ResponsibleUserId, string Recommendation, decimal? RecommendedPracticalHours, int? RecommendedSessions,
    DateOnly ReviewDate, IReadOnlyCollection<Guid> CompetencyIds, ExamFailureAnalysisId SourceAnalysisId,
    ExamResultId ResultId, int ResultRevision);
public sealed record ExamRemediationProvisionResult(bool Success, bool Deferred, RemediationPlanId? PlanId = null,
    string? Code = null, string? Detail = null);
public sealed record ExamRemediationPedagogicalStatus(string Status);

public interface IExamRemediationGateway
{
    Task<ExamRemediationProvisionResult> ProvisionAsync(ExamRemediationProvisionRequest request, CancellationToken ct = default);
    Task<ExamRemediationPedagogicalStatus?> GetStatusAsync(OrganizationId organizationId, RemediationPlanId planId, CancellationToken ct = default);
}
