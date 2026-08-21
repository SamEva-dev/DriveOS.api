using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Domain.Sessions;

public interface ITrainingSessionRepository
{
    Task<TrainingSession?> GetByIdAsync(
        OrganizationId organizationId,
        TrainingSessionId sessionId,
        CancellationToken cancellationToken = default);

    Task<TrainingSession?> GetByIdForUpdateAsync(
        OrganizationId organizationId,
        TrainingSessionId sessionId,
        CancellationToken cancellationToken = default);

    Task<TrainingSession?> GetBySourceBookingAsync(
        OrganizationId organizationId,
        BookingId bookingId,
        CancellationToken cancellationToken = default);

    Task<TrainingSession?> GetBySourceBookingForUpdateAsync(
        OrganizationId organizationId,
        BookingId bookingId,
        CancellationToken cancellationToken = default);

    void Add(TrainingSession session);
}
