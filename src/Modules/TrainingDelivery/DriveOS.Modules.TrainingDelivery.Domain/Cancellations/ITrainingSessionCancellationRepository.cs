using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Domain.Cancellations;

public interface ITrainingSessionCancellationRepository
{
    Task<SessionCancellation?> GetByIdAsync(
        OrganizationId organizationId,
        SessionCancellationId cancellationId,
        CancellationToken cancellationToken = default);

    Task<SessionCancellation?> GetByIdForUpdateAsync(
        OrganizationId organizationId,
        SessionCancellationId cancellationId,
        CancellationToken cancellationToken = default);

    Task<SessionCancellation?> GetBySessionAsync(
        OrganizationId organizationId,
        TrainingSessionId sessionId,
        CancellationToken cancellationToken = default);

    Task<SessionCancellation?> GetBySessionForUpdateAsync(
        OrganizationId organizationId,
        TrainingSessionId sessionId,
        CancellationToken cancellationToken = default);

    void Add(SessionCancellation cancellation);
}
