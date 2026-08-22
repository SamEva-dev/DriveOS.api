using System.Security.Cryptography;
using System.Text;
using DriveOS.Modules.ExamsCertification.Domain.Readiness.Opinions.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Domain.Readiness.Opinions;

/// <summary>
/// Immutable, versioned pedagogical opinion about presenting a student for an exam.
/// The aggregate records the human judgement and a server-generated evidence snapshot; it does not
/// become the source of truth for competencies, assessments or pedagogical reviews owned by BC-08.
/// Multiple authors may hold concurrent opinions, while a new submission by the same author creates
/// a new version linked to the previous one rather than overwriting history.
/// </summary>
public sealed class ExamReadinessOpinion : AggregateRoot<ExamReadinessOpinionId>, IAuditableEntity
{
    private ExamReadinessOpinion() { }

    private ExamReadinessOpinion(
        ExamReadinessOpinionId id,
        OrganizationId organizationId,
        PersonId studentId,
        TrainingPathId trainingPathId,
        ExamReadinessOpinionId? previousOpinionId,
        int version,
        ExamReadinessOpinionType opinion,
        ObservedAutonomyLevel observedAutonomy,
        string reservationCodes,
        string? reservations,
        string? conditions,
        string? comment,
        decimal progressPercent,
        int requiredCompetencies,
        int evaluatedRequiredCompetencies,
        bool hasCompletedPedagogicalReview,
        string? latestPedagogicalDecision,
        Guid operationId,
        string requestFingerprint,
        UserId authorId,
        DateTimeOffset submittedAtUtc) : base(id)
    {
        OrganizationId = organizationId;
        StudentId = studentId;
        TrainingPathId = trainingPathId;
        PreviousOpinionId = previousOpinionId;
        Version = version;
        Opinion = opinion;
        ObservedAutonomy = observedAutonomy;
        ReservationCodesSerialized = reservationCodes;
        Reservations = reservations;
        Conditions = conditions;
        Comment = comment;
        ProgressPercent = progressPercent;
        RequiredCompetencies = requiredCompetencies;
        EvaluatedRequiredCompetencies = evaluatedRequiredCompetencies;
        HasCompletedPedagogicalReview = hasCompletedPedagogicalReview;
        LatestPedagogicalDecision = latestPedagogicalDecision;
        OperationId = operationId;
        RequestFingerprint = requestFingerprint;
        AuthorId = authorId;
        SubmittedAtUtc = submittedAtUtc.ToUniversalTime();
    }

    public OrganizationId OrganizationId { get; private set; }
    public PersonId StudentId { get; private set; }
    public TrainingPathId TrainingPathId { get; private set; }
    public ExamReadinessOpinionId? PreviousOpinionId { get; private set; }
    public int Version { get; private set; }
    public ExamReadinessOpinionType Opinion { get; private set; }
    public ObservedAutonomyLevel ObservedAutonomy { get; private set; }
    public string ReservationCodesSerialized { get; private set; } = string.Empty;
    public string? Reservations { get; private set; }
    public string? Conditions { get; private set; }
    public string? Comment { get; private set; }

    /// <summary>BC-08 readiness snapshot at the exact time the human opinion was submitted.</summary>
    public decimal ProgressPercent { get; private set; }
    public int RequiredCompetencies { get; private set; }
    public int EvaluatedRequiredCompetencies { get; private set; }
    public bool HasCompletedPedagogicalReview { get; private set; }
    public string? LatestPedagogicalDecision { get; private set; }

    public Guid OperationId { get; private set; }
    public string RequestFingerprint { get; private set; } = string.Empty;
    public UserId AuthorId { get; private set; }
    public DateTimeOffset SubmittedAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public IReadOnlyCollection<ExamReadinessReservationCode> ReservationCodes =>
        string.IsNullOrWhiteSpace(ReservationCodesSerialized)
            ? Array.Empty<ExamReadinessReservationCode>()
            : ReservationCodesSerialized.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => Enum.Parse<ExamReadinessReservationCode>(x, true))
                .ToArray();

    public static Result<ExamReadinessOpinion> Submit(
        ExamReadinessOpinionId id,
        OrganizationId organizationId,
        PersonId studentId,
        TrainingPathId trainingPathId,
        ExamReadinessOpinionId? previousOpinionId,
        int version,
        ExamReadinessOpinionType opinion,
        ObservedAutonomyLevel observedAutonomy,
        IReadOnlyCollection<ExamReadinessReservationCode> reservationCodes,
        string? reservations,
        string? conditions,
        string? comment,
        decimal progressPercent,
        int requiredCompetencies,
        int evaluatedRequiredCompetencies,
        bool hasCompletedPedagogicalReview,
        string? latestPedagogicalDecision,
        Guid operationId,
        UserId authorId,
        DateTimeOffset submittedAtUtc)
    {
        if (id.IsEmpty) return Result.Failure<ExamReadinessOpinion>(ExamReadinessOpinionErrors.InvalidIdentifier);
        if (organizationId.IsEmpty) return Result.Failure<ExamReadinessOpinion>(ExamReadinessOpinionErrors.InvalidOrganization);
        if (studentId.IsEmpty) return Result.Failure<ExamReadinessOpinion>(ExamReadinessOpinionErrors.InvalidStudent);
        if (trainingPathId.IsEmpty) return Result.Failure<ExamReadinessOpinion>(ExamReadinessOpinionErrors.InvalidTrainingPath);
        if (authorId.IsEmpty) return Result.Failure<ExamReadinessOpinion>(ExamReadinessOpinionErrors.InvalidAuthor);
        if (operationId == Guid.Empty) return Result.Failure<ExamReadinessOpinion>(ExamReadinessOpinionErrors.InvalidOperation);
        if (version <= 0) return Result.Failure<ExamReadinessOpinion>(ExamReadinessOpinionErrors.InvalidVersion);
        if (!Enum.IsDefined(opinion)) return Result.Failure<ExamReadinessOpinion>(ExamReadinessOpinionErrors.InvalidOpinion);
        if (!Enum.IsDefined(observedAutonomy)) return Result.Failure<ExamReadinessOpinion>(ExamReadinessOpinionErrors.InvalidAutonomy);
        if (comment?.Length > 4000 || reservations?.Length > 4000 || conditions?.Length > 4000)
            return Result.Failure<ExamReadinessOpinion>(ExamReadinessOpinionErrors.InvalidComment);

        var normalizedCodes = reservationCodes.Distinct().OrderBy(x => (int)x).ToArray();
        if (normalizedCodes.Any(x => !Enum.IsDefined(x)))
            return Result.Failure<ExamReadinessOpinion>(ExamReadinessOpinionErrors.InvalidOpinion);
        if (opinion == ExamReadinessOpinionType.FavorableWithReservations && normalizedCodes.Length == 0)
            return Result.Failure<ExamReadinessOpinion>(ExamReadinessOpinionErrors.ReservationsRequired);
        if (opinion == ExamReadinessOpinionType.FavorableWithReservations && string.IsNullOrWhiteSpace(conditions))
            return Result.Failure<ExamReadinessOpinion>(ExamReadinessOpinionErrors.ConditionsRequired);

        string codes = string.Join(',', normalizedCodes.Select(x => x.ToString()));
        string fingerprint = ComputeFingerprint(opinion, observedAutonomy, codes, reservations, conditions, comment, authorId);
        var entity = new ExamReadinessOpinion(
            id, organizationId, studentId, trainingPathId, previousOpinionId, version, opinion, observedAutonomy,
            codes, Normalize(reservations), Normalize(conditions), Normalize(comment), progressPercent, requiredCompetencies,
            evaluatedRequiredCompetencies, hasCompletedPedagogicalReview, Normalize(latestPedagogicalDecision), operationId,
            fingerprint, authorId, submittedAtUtc);

        entity.RaiseDomainEvent(new ExamReadinessOpinionSubmittedDomainEvent(id, organizationId, studentId, trainingPathId, opinion, authorId, version));
        if (opinion == ExamReadinessOpinionType.SecondOpinionRequested)
            entity.RaiseDomainEvent(new ExamReadinessSecondOpinionRequestedDomainEvent(id, organizationId, studentId, trainingPathId, authorId));
        return Result.Success(entity);
    }

    public bool IsReplayOf(string requestFingerprint) =>
        string.Equals(RequestFingerprint, requestFingerprint, StringComparison.Ordinal);

    public static string CreateRequestFingerprint(
        ExamReadinessOpinionType opinion,
        ObservedAutonomyLevel observedAutonomy,
        IReadOnlyCollection<ExamReadinessReservationCode> reservationCodes,
        string? reservations,
        string? conditions,
        string? comment,
        UserId authorId)
    {
        string codes = string.Join(',', reservationCodes.Distinct().OrderBy(x => (int)x).Select(x => x.ToString()));
        return ComputeFingerprint(opinion, observedAutonomy, codes, reservations, conditions, comment, authorId);
    }

    public void SetCreatedAudit(DateTimeOffset atUtc, UserId? byUserId)
    {
        if (CreatedAtUtc != default) return;
        CreatedAtUtc = atUtc.ToUniversalTime();
        CreatedByUserId = byUserId;
    }

    public void SetModifiedAudit(DateTimeOffset atUtc, UserId? byUserId)
    {
        LastModifiedAtUtc = atUtc.ToUniversalTime();
        LastModifiedByUserId = byUserId;
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string ComputeFingerprint(
        ExamReadinessOpinionType opinion,
        ObservedAutonomyLevel autonomy,
        string codes,
        string? reservations,
        string? conditions,
        string? comment,
        UserId authorId)
    {
        string payload = $"{opinion}|{autonomy}|{codes}|{Normalize(reservations)}|{Normalize(conditions)}|{Normalize(comment)}|{authorId.Value}";
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload)));
    }
}
