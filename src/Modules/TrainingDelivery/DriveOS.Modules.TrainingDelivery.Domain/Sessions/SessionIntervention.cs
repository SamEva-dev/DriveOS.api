using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.TrainingDelivery.Domain.Sessions;

public sealed class SessionIntervention : Entity<TrainingSessionInterventionId>
{
    private SessionIntervention() { }
    private SessionIntervention(TrainingSessionInterventionId id) : base(id) { }

    public TrainingSessionId TrainingSessionId { get; private set; }
    public Guid OperationId { get; private set; }
    public string RequestFingerprint { get; private set; } = string.Empty;
    public TrainingSessionInterventionType Type { get; private set; }
    public TrainingSessionInterventionSeverity Severity { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }
    public string Context { get; private set; } = string.Empty;
    public string Reason { get; private set; } = string.Empty;
    public CompetencyId? RelatedCompetencyId { get; private set; }
    public string? Outcome { get; private set; }
    public string? InternalComment { get; private set; }
    public string? SharedExplanation { get; private set; }
    public UserId RecordedByUserId { get; private set; }
    public DateTimeOffset RecordedAtUtc { get; private set; }

    internal static Result<SessionIntervention> Create(
        TrainingSessionInterventionId id, TrainingSessionId sessionId, Guid operationId, string fingerprint,
        TrainingSessionInterventionType type, TrainingSessionInterventionSeverity severity, DateTimeOffset occurredAtUtc,
        string context, string reason, CompetencyId? relatedCompetencyId, string? outcome, string? internalComment,
        string? sharedExplanation, UserId actor, DateTimeOffset recordedAtUtc)
    {
        if (id.IsEmpty || sessionId.IsEmpty || operationId == Guid.Empty || actor.IsEmpty || !Enum.IsDefined(type) || !Enum.IsDefined(severity))
            return Result.Failure<SessionIntervention>(TrainingSessionErrors.InterventionInvalid);
        if (string.IsNullOrWhiteSpace(fingerprint) || fingerprint.Length > 64 || string.IsNullOrWhiteSpace(context) || string.IsNullOrWhiteSpace(reason))
            return Result.Failure<SessionIntervention>(TrainingSessionErrors.InterventionInvalid);
        if (context.Trim().Length > 1000 || reason.Trim().Length > 1000 || outcome?.Trim().Length > 1000 || internalComment?.Trim().Length > 2000 || sharedExplanation?.Trim().Length > 1000)
            return Result.Failure<SessionIntervention>(TrainingSessionErrors.InterventionTextTooLong);

        return Result.Success(new SessionIntervention(id)
        {
            TrainingSessionId = sessionId, OperationId = operationId, RequestFingerprint = fingerprint, Type = type, Severity = severity,
            OccurredAtUtc = occurredAtUtc.ToUniversalTime(), Context = context.Trim(), Reason = reason.Trim(), RelatedCompetencyId = relatedCompetencyId,
            Outcome = Normalize(outcome), InternalComment = Normalize(internalComment), SharedExplanation = Normalize(sharedExplanation),
            RecordedByUserId = actor, RecordedAtUtc = recordedAtUtc.ToUniversalTime()
        });
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
