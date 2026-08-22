using DriveOS.Modules.ExamsCertification.Domain.Readiness;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Application.Readiness;

public sealed record ExamReadinessSourceCheck(
    string Code,
    string MessageKey,
    ExamReadinessCheckStatus Status,
    string Source,
    string? Detail = null);

public sealed record ExamReadinessSnapshot(
    Guid StudentId,
    Guid TrainingPathId,
    DateTimeOffset EvaluatedAtUtc,
    ExamReadinessCheckStatus PedagogicalCheck,
    ExamReadinessCheckStatus AdministrativeCheck,
    ExamReadinessCheckStatus FinancialCheck,
    ExamReadinessCheckStatus RegulatoryCheck,
    int CompletedTrainingSessions,
    decimal DeliveredTrainingMinutes,
    IReadOnlyCollection<ExamReadinessSourceCheck> Checks)
{
    public bool HasBlockingCheck =>
        PedagogicalCheck == ExamReadinessCheckStatus.Blocked
        || AdministrativeCheck == ExamReadinessCheckStatus.Blocked
        || FinancialCheck == ExamReadinessCheckStatus.Blocked
        || RegulatoryCheck == ExamReadinessCheckStatus.Blocked;

    public bool IsFullySatisfied =>
        IsSatisfied(PedagogicalCheck)
        && IsSatisfied(AdministrativeCheck)
        && IsSatisfied(FinancialCheck)
        && IsSatisfied(RegulatoryCheck);

    private static bool IsSatisfied(ExamReadinessCheckStatus status) =>
        status is ExamReadinessCheckStatus.Satisfied or ExamReadinessCheckStatus.NotApplicable;
}

public interface IExamReadinessSnapshotGateway
{
    Task<Result<ExamReadinessSnapshot>> EvaluateAsync(
        OrganizationId organizationId,
        PersonId studentId,
        TrainingPathId trainingPathId,
        CancellationToken cancellationToken = default);
}

public sealed record GetExamReadinessSnapshotQuery(
    OrganizationId OrganizationId,
    PersonId StudentId,
    TrainingPathId TrainingPathId)
    : DriveOS.Application.Abstractions.Messaging.IQuery<ExamReadinessSnapshot>;

public sealed class GetExamReadinessSnapshotQueryHandler(IExamReadinessSnapshotGateway gateway)
    : DriveOS.Application.Abstractions.Messaging.IQueryHandler<GetExamReadinessSnapshotQuery, ExamReadinessSnapshot>
{
    public Task<Result<ExamReadinessSnapshot>> Handle(
        GetExamReadinessSnapshotQuery query,
        CancellationToken cancellationToken) =>
        gateway.EvaluateAsync(query.OrganizationId, query.StudentId, query.TrainingPathId, cancellationToken);
}
