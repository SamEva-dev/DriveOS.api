using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Domain.Sessions;

public sealed record TrainingSessionCancellationFacts(
    DateTimeOffset ActualStartAtUtc,
    DateTimeOffset ActualEndAtUtc,
    int GrossDurationMinutes,
    int InterruptionDurationMinutes,
    int DeliveredDurationMinutes,
    decimal? DistanceKilometers,
    UserId InstructorId,
    Guid? VehicleId,
    BranchId? BranchId);
