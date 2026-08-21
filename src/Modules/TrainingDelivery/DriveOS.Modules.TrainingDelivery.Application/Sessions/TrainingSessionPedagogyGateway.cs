using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.TrainingDelivery.Application.Sessions;

public sealed record TrainingSessionPedagogyAssessmentRequest(
    OrganizationId OrganizationId,
    TrainingPathId TrainingPathId,
    TrainingSessionId SessionId,
    Guid OperationId,
    CompetencyId CompetencyId,
    string LevelCode,
    string? InternalComment,
    string? SharedComment,
    DateTimeOffset AssessedAtUtc,
    UserId AssessorUserId);

public sealed record TrainingSessionPedagogyAssessmentReference(
    Guid PedagogyAssessmentId,
    CurriculumVersionId CurriculumVersionId);

public interface ITrainingSessionPedagogyGateway
{
    Task<Result<TrainingSessionPedagogyAssessmentReference>> RecordAssessmentAsync(
        TrainingSessionPedagogyAssessmentRequest request,
        CancellationToken cancellationToken = default);
}
