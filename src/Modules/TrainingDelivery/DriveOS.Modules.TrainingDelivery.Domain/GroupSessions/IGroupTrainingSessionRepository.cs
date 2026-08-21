using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Domain.GroupSessions;

public interface IGroupTrainingSessionRepository
{
    Task<GroupTrainingSession?> GetByIdAsync(
        OrganizationId organizationId,
        GroupTrainingSessionId id,
        CancellationToken cancellationToken = default);

    Task<GroupTrainingSession?> GetByIdForUpdateAsync(
        OrganizationId organizationId,
        GroupTrainingSessionId id,
        CancellationToken cancellationToken = default);

    Task<GroupTrainingSession?> GetBySourceBookingAsync(
        OrganizationId organizationId,
        BookingId bookingId,
        CancellationToken cancellationToken = default);

    Task<GroupTrainingSession?> GetBySourceBookingForUpdateAsync(
        OrganizationId organizationId,
        BookingId bookingId,
        CancellationToken cancellationToken = default);

    void Add(GroupTrainingSession session);
}
