using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.TrainingDelivery.Domain.Sessions;

public sealed class SessionCompetencyAssessment : Entity<TrainingSessionCompetencyAssessmentId>
{
    private SessionCompetencyAssessment() { }

    private SessionCompetencyAssessment(
        TrainingSessionCompetencyAssessmentId id,
        TrainingSessionId trainingSessionId,
        Guid operationId,
        string requestFingerprint,
        CompetencyId competencyId,
        CurriculumVersionId curriculumVersionId,
        Guid pedagogyAssessmentId,
        string levelCode,
        string? observedCriteria,
        string? context,
        TrainingSessionInterventionId? relatedInterventionId,
        string? internalComment,
        string? sharedComment,
        Guid? evidenceDocumentId,
        DateTimeOffset assessedAtUtc,
        UserId assessorUserId,
        DateTimeOffset recordedAtUtc)
        : base(id)
    {
        TrainingSessionId = trainingSessionId;
        OperationId = operationId;
        RequestFingerprint = requestFingerprint;
        CompetencyId = competencyId;
        CurriculumVersionId = curriculumVersionId;
        PedagogyAssessmentId = pedagogyAssessmentId;
        LevelCode = levelCode;
        ObservedCriteria = observedCriteria;
        Context = context;
        RelatedInterventionId = relatedInterventionId;
        InternalComment = internalComment;
        SharedComment = sharedComment;
        EvidenceDocumentId = evidenceDocumentId;
        AssessedAtUtc = assessedAtUtc;
        AssessorUserId = assessorUserId;
        RecordedAtUtc = recordedAtUtc;
    }

    public TrainingSessionId TrainingSessionId { get; private set; }
    public Guid OperationId { get; private set; }
    public string RequestFingerprint { get; private set; } = string.Empty;
    public CompetencyId CompetencyId { get; private set; }
    public CurriculumVersionId CurriculumVersionId { get; private set; }
    public Guid PedagogyAssessmentId { get; private set; }
    public string LevelCode { get; private set; } = string.Empty;
    public string? ObservedCriteria { get; private set; }
    public string? Context { get; private set; }
    public TrainingSessionInterventionId? RelatedInterventionId { get; private set; }
    public string? InternalComment { get; private set; }
    public string? SharedComment { get; private set; }
    public Guid? EvidenceDocumentId { get; private set; }
    public DateTimeOffset AssessedAtUtc { get; private set; }
    public UserId AssessorUserId { get; private set; }
    public DateTimeOffset RecordedAtUtc { get; private set; }

    internal static Result<SessionCompetencyAssessment> Create(
        TrainingSessionCompetencyAssessmentId id,
        TrainingSessionId sessionId,
        Guid operationId,
        string fingerprint,
        CompetencyId competencyId,
        CurriculumVersionId curriculumVersionId,
        Guid pedagogyAssessmentId,
        string levelCode,
        string? observedCriteria,
        string? context,
        TrainingSessionInterventionId? relatedInterventionId,
        string? internalComment,
        string? sharedComment,
        Guid? evidenceDocumentId,
        DateTimeOffset assessedAtUtc,
        UserId assessorUserId,
        DateTimeOffset recordedAtUtc)
    {
        if (id.IsEmpty || sessionId.IsEmpty || operationId == Guid.Empty || competencyId.IsEmpty || curriculumVersionId.IsEmpty || pedagogyAssessmentId == Guid.Empty || assessorUserId.IsEmpty)
            return Result.Failure<SessionCompetencyAssessment>(TrainingSessionErrors.AssessmentInvalid);

        string level = (levelCode ?? string.Empty).Trim().ToUpperInvariant();
        if (level.Length is < 1 or > 60)
            return Result.Failure<SessionCompetencyAssessment>(TrainingSessionErrors.AssessmentLevelInvalid);
        if (observedCriteria?.Length > 4000 || context?.Length > 4000 || internalComment?.Length > 4000 || sharedComment?.Length > 4000)
            return Result.Failure<SessionCompetencyAssessment>(TrainingSessionErrors.AssessmentTextTooLong);

        return Result.Success(new SessionCompetencyAssessment(
            id, sessionId, operationId, fingerprint, competencyId, curriculumVersionId, pedagogyAssessmentId, level,
            Normalize(observedCriteria), Normalize(context), relatedInterventionId, Normalize(internalComment), Normalize(sharedComment), evidenceDocumentId,
            assessedAtUtc.ToUniversalTime(), assessorUserId, recordedAtUtc.ToUniversalTime()));
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
