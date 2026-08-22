using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Application.Results.Failure;

public sealed record ExamFailureFindingResponse(Guid Id, string Kind, string Code, string? Detail, bool Critical, string Source, Guid ActorUserId, DateTimeOffset CreatedAtUtc);
public sealed record ExamFailureAnalysisResponse(Guid Id, Guid ExamResultId, int ResultRevision, Guid AttemptId, Guid RegistrationId,
    Guid StudentId, int AttemptNumber, string Status, string? InstructorAnalysis, string? StudentFeedback, string? Summary,
    string? Recommendation, DateTimeOffset? CompletedAtUtc, Guid? CompletedByUserId, DateTimeOffset? SupersededAtUtc,
    IReadOnlyList<ExamFailureFindingResponse> Findings);

public sealed record GetExamFailureAnalysisQuery(OrganizationId OrganizationId, ExamResultId ResultId) : IQuery<ExamFailureAnalysisResponse>;
public sealed record AddExamFailureFindingCommand(OrganizationId OrganizationId, ExamResultId ResultId, int ResultRevision,
    string Kind, string Code, string? Detail, bool Critical, string Source, UserId ActorUserId) : ICommand<ExamFailureAnalysisResponse>;
public sealed record UpdateExamFailureNarrativeCommand(OrganizationId OrganizationId, ExamResultId ResultId, int ResultRevision,
    string? InstructorAnalysis, string? StudentFeedback, string? Recommendation, UserId ActorUserId) : ICommand<ExamFailureAnalysisResponse>;
public sealed record CompleteExamFailureAnalysisCommand(OrganizationId OrganizationId, ExamResultId ResultId, int ResultRevision,
    string Summary, string? Recommendation, UserId ActorUserId) : ICommand<ExamFailureAnalysisResponse>;
