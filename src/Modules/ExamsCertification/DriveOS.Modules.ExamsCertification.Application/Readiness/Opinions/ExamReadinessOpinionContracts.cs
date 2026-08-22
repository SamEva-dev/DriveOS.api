using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.ExamsCertification.Domain.Readiness.Opinions;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Application.Readiness.Opinions;

public sealed record ExamReadinessOpinionContext(
    Guid StudentId,
    Guid TrainingPathId,
    decimal ProgressPercent,
    int RequiredCompetencies,
    int EvaluatedRequiredCompetencies,
    bool CriticalCompetenciesValidated,
    bool HasCompletedPedagogicalReview,
    string? LatestPedagogicalDecision,
    IReadOnlyCollection<string> Blockers,
    DateTimeOffset EvaluatedAtUtc);

public interface IExamReadinessOpinionContextGateway
{
    Task<Result<ExamReadinessOpinionContext>> GetAsync(
        OrganizationId organizationId,
        PersonId studentId,
        TrainingPathId trainingPathId,
        CancellationToken cancellationToken = default);
}

public sealed record GetExamReadinessOpinionContextQuery(
    OrganizationId OrganizationId,
    PersonId StudentId,
    TrainingPathId TrainingPathId) : IQuery<ExamReadinessOpinionContext>;

public sealed record SubmitExamReadinessOpinionCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    TrainingPathId TrainingPathId,
    ExamReadinessOpinionType Opinion,
    ObservedAutonomyLevel ObservedAutonomy,
    IReadOnlyCollection<ExamReadinessReservationCode> ReservationCodes,
    string? Reservations,
    string? Conditions,
    string? Comment,
    Guid OperationId,
    UserId AuthorId) : ICommand<ExamReadinessOpinionId>;

public sealed record GetExamReadinessOpinionsQuery(
    OrganizationId OrganizationId,
    PersonId StudentId,
    TrainingPathId TrainingPathId) : IQuery<IReadOnlyList<ExamReadinessOpinionResponse>>;

public sealed record ExamReadinessOpinionResponse(
    Guid Id,
    Guid StudentId,
    Guid TrainingPathId,
    Guid? PreviousOpinionId,
    int Version,
    string Opinion,
    string ObservedAutonomy,
    IReadOnlyCollection<string> ReservationCodes,
    string? Reservations,
    string? Conditions,
    string? Comment,
    decimal ProgressPercent,
    int RequiredCompetencies,
    int EvaluatedRequiredCompetencies,
    bool HasCompletedPedagogicalReview,
    string? LatestPedagogicalDecision,
    Guid AuthorId,
    DateTimeOffset SubmittedAtUtc);

public static class ExamReadinessOpinionApplicationErrors
{
    public static readonly Error ContextUnavailable = Error.Conflict(
        "Exams.Readiness.Opinion.ContextUnavailable",
        "errors.exams.readiness.opinion.contextUnavailable");
}
