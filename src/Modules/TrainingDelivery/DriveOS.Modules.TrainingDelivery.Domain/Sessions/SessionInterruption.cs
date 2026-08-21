using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.TrainingDelivery.Domain.Sessions;

public sealed class SessionInterruption : Entity<TrainingSessionInterruptionId>
{
    private SessionInterruption() { }
    private SessionInterruption(TrainingSessionInterruptionId id) : base(id) { }

    public TrainingSessionId TrainingSessionId { get; private set; }
    public Guid InterruptOperationId { get; private set; }
    public string InterruptRequestFingerprint { get; private set; } = string.Empty;
    public TrainingSessionInterruptionReason Reason { get; private set; }
    public string? Description { get; private set; }
    public DateTimeOffset StartedAtUtc { get; private set; }
    public UserId InterruptedByUserId { get; private set; }
    public Guid? ResumeOperationId { get; private set; }
    public string? ResumeRequestFingerprint { get; private set; }
    public DateTimeOffset? ResumedAtUtc { get; private set; }
    public UserId? ResumedByUserId { get; private set; }
    public DateTimeOffset? TerminatedAtUtc { get; private set; }
    public SessionCancellationId? TerminatedByCancellationId { get; private set; }

    public bool IsActive => !ResumedAtUtc.HasValue && !TerminatedAtUtc.HasValue;

    internal static Result<SessionInterruption> Create(
        TrainingSessionInterruptionId id,
        TrainingSessionId sessionId,
        Guid operationId,
        string fingerprint,
        TrainingSessionInterruptionReason reason,
        string? description,
        DateTimeOffset startedAtUtc,
        UserId actor)
    {
        if (id.IsEmpty || sessionId.IsEmpty || operationId == Guid.Empty || actor.IsEmpty || !Enum.IsDefined(reason))
            return Result.Failure<SessionInterruption>(TrainingSessionErrors.InterruptionInvalid);
        if (description?.Length > 3000)
            return Result.Failure<SessionInterruption>(TrainingSessionErrors.InterruptionDescriptionTooLong);

        return Result.Success(new SessionInterruption(id)
        {
            TrainingSessionId = sessionId,
            InterruptOperationId = operationId,
            InterruptRequestFingerprint = fingerprint,
            Reason = reason,
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            StartedAtUtc = startedAtUtc.ToUniversalTime(),
            InterruptedByUserId = actor
        });
    }

    internal Result Resume(Guid operationId, string fingerprint, DateTimeOffset resumedAtUtc, UserId actor)
    {
        if (operationId == Guid.Empty || actor.IsEmpty || string.IsNullOrWhiteSpace(fingerprint))
            return Result.Failure(TrainingSessionErrors.InterruptionInvalid);
        if (TerminatedAtUtc.HasValue)
            return Result.Failure(TrainingSessionErrors.InterruptionAlreadyResumed);
        if (ResumedAtUtc.HasValue)
        {
            return ResumeOperationId == operationId && ResumeRequestFingerprint == fingerprint
                ? Result.Success()
                : Result.Failure(TrainingSessionErrors.InterruptionAlreadyResumed);
        }

        DateTimeOffset normalized = resumedAtUtc.ToUniversalTime();
        if (normalized < StartedAtUtc)
            return Result.Failure(TrainingSessionErrors.InterruptionResumeBeforeStart);

        ResumeOperationId = operationId;
        ResumeRequestFingerprint = fingerprint;
        ResumedAtUtc = normalized;
        ResumedByUserId = actor;
        return Result.Success();
    }

    internal Result Terminate(SessionCancellationId cancellationId, DateTimeOffset terminatedAtUtc)
    {
        if (cancellationId.IsEmpty) return Result.Failure(TrainingSessionErrors.InterruptionInvalid);
        if (!IsActive) return Result.Success();
        DateTimeOffset normalized = terminatedAtUtc.ToUniversalTime();
        if (normalized < StartedAtUtc) return Result.Failure(TrainingSessionErrors.InterruptionResumeBeforeStart);
        TerminatedAtUtc = normalized;
        TerminatedByCancellationId = cancellationId;
        return Result.Success();
    }
}
