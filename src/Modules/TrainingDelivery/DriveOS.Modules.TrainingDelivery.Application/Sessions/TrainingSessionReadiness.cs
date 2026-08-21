using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.TrainingDelivery.Application.Sessions;

public enum TrainingSessionReadinessCheckStatus
{
    Passed = 1,
    Blocked = 2,
    ExternalReview = 3,
    NotApplicable = 4
}

public sealed record TrainingSessionReadinessCheck(string Code, string MessageKey, TrainingSessionReadinessCheckStatus Status, string? Detail = null);

public sealed record TrainingSessionExecutionReadiness(
    bool IsReady,
    BookingId BookingId,
    PersonId StudentId,
    TrainingPathId TrainingPathId,
    UserId InstructorId,
    BranchId? BranchId,
    Guid? VehicleId,
    DateTimeOffset PlannedStartAtUtc,
    DateTimeOffset PlannedEndAtUtc,
    IReadOnlyCollection<TrainingSessionReadinessCheck> Checks);

public interface ITrainingSessionExecutionReadinessGateway
{
    Task<Result<TrainingSessionExecutionReadiness>> CheckAsync(
        OrganizationId organizationId,
        BookingId bookingId,
        CancellationToken cancellationToken = default);
}

public interface ITrainingSessionExecutionLock
{
    Task AcquireAsync(
        OrganizationId organizationId,
        TrainingSessionId sessionId,
        CancellationToken cancellationToken = default);
}
