using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.ExamsCertification.Domain.Readiness;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Application.Readiness;

public sealed record RecordExamReadinessDecisionCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    TrainingPathId TrainingPathId,
    ExamReadinessOutcome Outcome,
    string Rationale,
    string? Conditions,
    UserId ReviewerId) : ICommand<ExamReadinessDecisionId>;

public sealed record GetExamReadinessDecisionQuery(
    OrganizationId OrganizationId,
    PersonId StudentId,
    TrainingPathId TrainingPathId) : IQuery<ExamReadinessDecisionResponse>;

public sealed record ExamReadinessDecisionResponse(
    Guid Id,
    Guid StudentId,
    Guid TrainingPathId,
    int Version,
    string Outcome,
    string PedagogicalCheck,
    string AdministrativeCheck,
    string FinancialCheck,
    string RegulatoryCheck,
    string Rationale,
    string? Conditions,
    Guid ReviewerId,
    DateTimeOffset DecidedAtUtc,
    bool IsCurrent,
    Guid? SupersededByDecisionId,
    DateTimeOffset? SupersededAtUtc);

public static class ExamReadinessApplicationErrors
{
    public static readonly Error DecisionNotFound = Error.NotFound(
        "Exams.Readiness.Decision.NotFound",
        "errors.exams.readiness.decision.notFound");

    public static readonly Error ConcurrencyConflict = Error.Conflict(
        "Exams.Readiness.Decision.ConcurrencyConflict",
        "errors.exams.readiness.decision.concurrencyConflict");

    public static readonly Error SnapshotUnavailable = Error.Conflict(
        "Exams.Readiness.Snapshot.Unavailable",
        "errors.exams.readiness.snapshot.unavailable");

    public static readonly Error StudentNotFound = Error.NotFound(
        "Exams.Readiness.Student.NotFound",
        "errors.exams.readiness.student.notFound");

    public static readonly Error TrainingPathNotFound = Error.NotFound(
        "Exams.Readiness.TrainingPath.NotFound",
        "errors.exams.readiness.trainingPath.notFound");

    public static readonly Error TrainingPathStudentMismatch = Error.Conflict(
        "Exams.Readiness.TrainingPath.StudentMismatch",
        "errors.exams.readiness.trainingPath.studentMismatch");
}
