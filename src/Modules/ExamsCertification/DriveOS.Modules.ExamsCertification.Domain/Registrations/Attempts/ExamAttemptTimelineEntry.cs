using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Domain.Registrations.Attempts;

/// <summary>Append-only fact for offline-safe and auditable exam-day operations.</summary>
public sealed class ExamAttemptTimelineEntry : Entity<ExamAttemptTimelineEntryId>
{
    private ExamAttemptTimelineEntry() { }

    internal ExamAttemptTimelineEntry(ExamAttemptTimelineEntryId id, ExamAttemptId attemptId, OrganizationId organizationId,
        Guid operationId, string requestFingerprint, ExamAttemptTimelineEntryType type, ExamAttemptStatus status, string? note,
        DateTimeOffset occurredAtUtc, UserId actorUserId, decimal? latitude = null, decimal? longitude = null,
        decimal? accuracyMeters = null, ExamAttemptLocationPurpose? locationPurpose = null, UserId? instructorId = null, VehicleId? vehicleId = null) : base(id)
    {
        AttemptId = attemptId; OrganizationId = organizationId; OperationId = operationId; RequestFingerprint = requestFingerprint;
        Type = type; Status = status; Note = note; OccurredAtUtc = occurredAtUtc.ToUniversalTime(); ActorUserId = actorUserId;
        Latitude = latitude; Longitude = longitude; AccuracyMeters = accuracyMeters; LocationPurpose = locationPurpose;
        InstructorId = instructorId; VehicleId = vehicleId;
    }

    public ExamAttemptId AttemptId { get; private set; }
    public OrganizationId OrganizationId { get; private set; }
    public Guid OperationId { get; private set; }
    public string RequestFingerprint { get; private set; } = string.Empty;
    public ExamAttemptTimelineEntryType Type { get; private set; }
    public ExamAttemptStatus Status { get; private set; }
    public string? Note { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }
    public UserId ActorUserId { get; private set; }
    public decimal? Latitude { get; private set; }
    public decimal? Longitude { get; private set; }
    public decimal? AccuracyMeters { get; private set; }
    public ExamAttemptLocationPurpose? LocationPurpose { get; private set; }
    public UserId? InstructorId { get; private set; }
    public VehicleId? VehicleId { get; private set; }
}
