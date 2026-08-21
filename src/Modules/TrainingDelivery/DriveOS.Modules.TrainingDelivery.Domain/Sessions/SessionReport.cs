using System.Security.Cryptography;
using System.Text;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.TrainingDelivery.Domain.Sessions;

/// <summary>
/// Pedagogical report attached to one executed training session. The report starts as an editable draft,
/// is versioned on every server save and is later submitted/validated by the dedicated review workflow.
/// </summary>
public sealed class SessionReport : Entity<TrainingSessionReportId>
{
    private readonly List<SessionReportNarrativeRevision> _narrativeRevisions = [];
    private readonly List<SessionReportRevision> _revisions = [];

    private SessionReport() { }
    private SessionReport(TrainingSessionReportId id) : base(id) { }

    public TrainingSessionId TrainingSessionId { get; private set; }
    public Guid OperationId { get; private set; }
    public string RequestFingerprint { get; private set; } = string.Empty;
    public SessionReportStatus Status { get; private set; }
    public int Version { get; private set; }
    public int LastCompletedStep { get; private set; }
    public DateTimeOffset ActualEndAtUtc { get; private set; }
    public int GrossDurationMinutes { get; private set; }
    public int InterruptionDurationMinutes { get; private set; }
    public int DeliveredDurationMinutes { get; private set; }
    public decimal? DistanceKilometers { get; private set; }
    public string Summary { get; private set; } = string.Empty;
    public string? ObjectivesWorked { get; private set; }
    public string? ObjectivesAchieved { get; private set; }
    public string? NextObjective { get; private set; }
    public string? SharedComment { get; private set; }
    public string? InternalNote { get; private set; }
    public string? InstructorComments { get; private set; }
    public IReadOnlyCollection<SessionReportNarrativeRevision> NarrativeRevisions => _narrativeRevisions.AsReadOnly();
    public IReadOnlyCollection<SessionReportRevision> Revisions => _revisions.AsReadOnly();
    public int? CorrectedDeliveredDurationMinutes { get; private set; }
    public UserId LastSavedByUserId { get; private set; }
    public DateTimeOffset LastSavedAtUtc { get; private set; }
    public UserId CompletedByUserId { get; private set; }
    public DateTimeOffset CompletedAtUtc { get; private set; }

    internal static Result<SessionReport> CreateDraft(
        TrainingSessionReportId id,
        TrainingSessionId sessionId,
        Guid operationId,
        DateTimeOffset actualEndAtUtc,
        int grossDurationMinutes,
        int interruptionDurationMinutes,
        int deliveredDurationMinutes,
        decimal? distanceKilometers,
        int lastCompletedStep,
        string? summary,
        string? objectivesWorked,
        string? objectivesAchieved,
        string? nextObjective,
        string? sharedComment,
        string? internalNote,
        UserId actor,
        DateTimeOffset savedAtUtc)
    {
        Result validation = ValidateDraft(lastCompletedStep, summary, objectivesWorked, objectivesAchieved, nextObjective, sharedComment, internalNote);
        if (validation.IsFailure) return Result.Failure<SessionReport>(validation.Error);
        if (id.IsEmpty || sessionId.IsEmpty || operationId == Guid.Empty || actor.IsEmpty)
            return Result.Failure<SessionReport>(TrainingSessionErrors.ReportDraftInvalid);

        string fingerprint = BuildFingerprint(lastCompletedStep, summary, objectivesWorked, objectivesAchieved, nextObjective, sharedComment, internalNote);
        DateTimeOffset now = savedAtUtc.ToUniversalTime();
        SessionReport report = new(id)
        {
            TrainingSessionId = sessionId,
            OperationId = operationId,
            RequestFingerprint = fingerprint,
            Status = SessionReportStatus.Draft,
            Version = 1,
            LastCompletedStep = lastCompletedStep,
            ActualEndAtUtc = actualEndAtUtc.ToUniversalTime(),
            GrossDurationMinutes = grossDurationMinutes,
            InterruptionDurationMinutes = interruptionDurationMinutes,
            DeliveredDurationMinutes = deliveredDurationMinutes,
            DistanceKilometers = distanceKilometers,
            Summary = Normalize(summary, 5000) ?? string.Empty,
            ObjectivesWorked = Normalize(objectivesWorked, 4000),
            ObjectivesAchieved = Normalize(objectivesAchieved, 4000),
            NextObjective = Normalize(nextObjective, 2000),
            SharedComment = Normalize(sharedComment, 5000),
            InternalNote = Normalize(internalNote, 5000),
            InstructorComments = Normalize(internalNote, 5000),
            LastSavedByUserId = actor,
            LastSavedAtUtc = now,
            CompletedByUserId = actor,
            CompletedAtUtc = now
        };
        if (report.SharedComment is not null)
            report._narrativeRevisions.Add(SessionReportNarrativeRevision.Create(report.Id, SessionReportNarrativeKind.SharedComment, 1, report.SharedComment, actor, now));
        if (report.InternalNote is not null)
            report._narrativeRevisions.Add(SessionReportNarrativeRevision.Create(report.Id, SessionReportNarrativeKind.InternalNote, 1, report.InternalNote, actor, now));
        return Result.Success(report);
    }

    internal Result SaveDraft(
        Guid operationId,
        int expectedVersion,
        int lastCompletedStep,
        string? summary,
        string? objectivesWorked,
        string? objectivesAchieved,
        string? nextObjective,
        string? sharedComment,
        string? internalNote,
        UserId actor,
        DateTimeOffset savedAtUtc)
    {
        if (Status is SessionReportStatus.Submitted or SessionReportStatus.PendingSupervisorReview or SessionReportStatus.Validated)
            return Result.Failure(TrainingSessionErrors.ReportDraftLocked);
        if (operationId == Guid.Empty || actor.IsEmpty)
            return Result.Failure(TrainingSessionErrors.ReportDraftInvalid);

        Result validation = ValidateDraft(lastCompletedStep, summary, objectivesWorked, objectivesAchieved, nextObjective, sharedComment, internalNote);
        if (validation.IsFailure) return validation;

        string fingerprint = BuildFingerprint(lastCompletedStep, summary, objectivesWorked, objectivesAchieved, nextObjective, sharedComment, internalNote);
        if (OperationId == operationId)
            return RequestFingerprint == fingerprint ? Result.Success() : Result.Failure(TrainingSessionErrors.ReportDraftOperationConflict);
        if (expectedVersion != Version)
            return Result.Failure(TrainingSessionErrors.ReportDraftVersionConflict);

        string? nextSharedComment = Normalize(sharedComment, 5000);
        string? nextInternalNote = Normalize(internalNote, 5000);
        OperationId = operationId;
        RequestFingerprint = fingerprint;
        Version++;
        LastCompletedStep = Math.Max(LastCompletedStep, lastCompletedStep);
        Summary = Normalize(summary, 5000) ?? string.Empty;
        ObjectivesWorked = Normalize(objectivesWorked, 4000);
        ObjectivesAchieved = Normalize(objectivesAchieved, 4000);
        NextObjective = Normalize(nextObjective, 2000);
        if (!string.Equals(SharedComment, nextSharedComment, StringComparison.Ordinal))
            _narrativeRevisions.Add(SessionReportNarrativeRevision.Create(Id, SessionReportNarrativeKind.SharedComment, Version, nextSharedComment, actor, savedAtUtc));
        if (!string.Equals(InternalNote, nextInternalNote, StringComparison.Ordinal))
            _narrativeRevisions.Add(SessionReportNarrativeRevision.Create(Id, SessionReportNarrativeKind.InternalNote, Version, nextInternalNote, actor, savedAtUtc));
        SharedComment = nextSharedComment;
        InternalNote = nextInternalNote;
        InstructorComments = InternalNote;
        LastSavedByUserId = actor;
        LastSavedAtUtc = savedAtUtc.ToUniversalTime();
        if (Status == SessionReportStatus.RejectedForCorrection) Status = SessionReportStatus.Draft;
        return Result.Success();
    }

    internal Result UpdateNarrative(
        Guid operationId,
        int expectedVersion,
        SessionReportNarrativeKind kind,
        string? content,
        UserId actor,
        DateTimeOffset savedAtUtc)
    {
        if (Status is SessionReportStatus.Submitted or SessionReportStatus.PendingSupervisorReview or SessionReportStatus.Validated)
            return Result.Failure(TrainingSessionErrors.ReportDraftLocked);
        if (operationId == Guid.Empty || actor.IsEmpty)
            return Result.Failure(TrainingSessionErrors.ReportDraftInvalid);
        if (expectedVersion != Version)
            return Result.Failure(TrainingSessionErrors.ReportDraftVersionConflict);

        string? normalized = Normalize(content, 5000);
        if (!string.IsNullOrWhiteSpace(content) && normalized is null)
            return Result.Failure(TrainingSessionErrors.ReportDraftTextTooLong);

        string current = kind == SessionReportNarrativeKind.SharedComment ? SharedComment ?? string.Empty : InternalNote ?? string.Empty;
        string fingerprint = BuildFingerprint((int)kind, normalized);
        if (OperationId == operationId)
            return RequestFingerprint == fingerprint ? Result.Success() : Result.Failure(TrainingSessionErrors.ReportDraftOperationConflict);

        if (string.Equals(current, normalized ?? string.Empty, StringComparison.Ordinal))
            return Result.Success();

        OperationId = operationId;
        RequestFingerprint = fingerprint;
        Version++;
        if (kind == SessionReportNarrativeKind.SharedComment) SharedComment = normalized;
        else
        {
            InternalNote = normalized;
            InstructorComments = normalized;
        }
        _narrativeRevisions.Add(SessionReportNarrativeRevision.Create(Id, kind, Version, normalized, actor, savedAtUtc));
        LastSavedByUserId = actor;
        LastSavedAtUtc = savedAtUtc.ToUniversalTime();
        return Result.Success();
    }


    internal Result Submit(Guid operationId, int expectedVersion, bool requestSupervisorReview, UserId actor, DateTimeOffset submittedAtUtc)
    {
        if (operationId == Guid.Empty || actor.IsEmpty)
            return Result.Failure(TrainingSessionErrors.ReportSubmissionInvalid);

        string fingerprint = BuildFingerprint(requestSupervisorReview ? 1 : 0, expectedVersion.ToString());
        if (OperationId == operationId)
            return RequestFingerprint == fingerprint ? Result.Success() : Result.Failure(TrainingSessionErrors.ReportDraftOperationConflict);
        if (expectedVersion != Version)
            return Result.Failure(TrainingSessionErrors.ReportDraftVersionConflict);
        if (Status is SessionReportStatus.Submitted or SessionReportStatus.PendingSupervisorReview or SessionReportStatus.Validated)
            return Result.Failure(TrainingSessionErrors.ReportAlreadySubmitted);
        if (Status != SessionReportStatus.ReadyToSubmit)
            return Result.Failure(TrainingSessionErrors.ReportNotReadyToSubmit);

        OperationId = operationId;
        RequestFingerprint = fingerprint;
        Version++;
        Status = requestSupervisorReview ? SessionReportStatus.PendingSupervisorReview : SessionReportStatus.Submitted;
        LastCompletedStep = 9;
        LastSavedByUserId = actor;
        LastSavedAtUtc = submittedAtUtc.ToUniversalTime();
        CompletedByUserId = actor;
        CompletedAtUtc = submittedAtUtc.ToUniversalTime();
        return Result.Success();
    }

    internal Result MarkReadyToSubmit(Guid operationId, int expectedVersion, UserId actor, DateTimeOffset readyAtUtc)
    {
        if (operationId == Guid.Empty || actor.IsEmpty)
            return Result.Failure(TrainingSessionErrors.ReportSubmissionInvalid);
        if (Status is SessionReportStatus.Submitted or SessionReportStatus.PendingSupervisorReview or SessionReportStatus.Validated)
            return Result.Failure(TrainingSessionErrors.ReportDraftLocked);

        string fingerprint = BuildFingerprint(9, expectedVersion.ToString(), "ready");
        if (OperationId == operationId)
            return RequestFingerprint == fingerprint ? Result.Success() : Result.Failure(TrainingSessionErrors.ReportDraftOperationConflict);
        if (expectedVersion != Version)
            return Result.Failure(TrainingSessionErrors.ReportDraftVersionConflict);

        OperationId = operationId;
        RequestFingerprint = fingerprint;
        Version++;
        Status = SessionReportStatus.ReadyToSubmit;
        LastCompletedStep = 9;
        LastSavedByUserId = actor;
        LastSavedAtUtc = readyAtUtc.ToUniversalTime();
        return Result.Success();
    }

    internal Result<SessionReportRevision> RequestRevision(
        TrainingSessionReportRevisionId revisionId, Guid operationId, int expectedVersion, SessionReportRevisionScenario scenario, string fieldCode,
        string currentValue, string proposedValue, string reason, bool hasFinancialImpact, bool approvalRequired, UserId actor, DateTimeOffset now)
    {
        if (Status is not (SessionReportStatus.Submitted or SessionReportStatus.PendingSupervisorReview or SessionReportStatus.Validated or SessionReportStatus.RejectedForCorrection))
            return Result.Failure<SessionReportRevision>(TrainingSessionErrors.ReportRevisionRequiresSubmittedReport);
        if (expectedVersion != Version) return Result.Failure<SessionReportRevision>(TrainingSessionErrors.ReportRevisionVersionConflict);
        SessionReportRevision? existing = _revisions.FirstOrDefault(x => x.OperationId == operationId);
        if (existing is not null) return Result.Success(existing);
        Result<SessionReportRevision> created = SessionReportRevision.Create(revisionId, Id, operationId, scenario, fieldCode, currentValue, proposedValue, reason, hasFinancialImpact, approvalRequired, actor, now);
        if (created.IsFailure) return created;
        _revisions.Add(created.Value);
        return created;
    }

    internal Result DecideRevision(TrainingSessionReportRevisionId revisionId, bool approve, string? decisionReason, UserId actor, DateTimeOffset now)
    {
        SessionReportRevision? revision = _revisions.FirstOrDefault(x => x.Id == revisionId);
        if (revision is null) return Result.Failure(TrainingSessionErrors.ReportRevisionNotFound);
        if (!approve) return revision.Reject(actor, now, decisionReason ?? string.Empty);
        Version++;
        string code = revision.FieldCode.Trim().ToLowerInvariant();
        if (code is "summary") Summary = revision.ProposedValue;
        else if (code is "objectivesworked") ObjectivesWorked = revision.ProposedValue;
        else if (code is "objectivesachieved") ObjectivesAchieved = revision.ProposedValue;
        else if (code is "nextobjective") NextObjective = revision.ProposedValue;
        else if (code is "sharedcomment") SharedComment = revision.ProposedValue;
        else if (code is "delivereddurationminutes" && int.TryParse(revision.ProposedValue, out int corrected) && corrected > 0) CorrectedDeliveredDurationMinutes = corrected;
        LastSavedByUserId = actor; LastSavedAtUtc = now.ToUniversalTime();
        return revision.Approve(actor, now, Version, decisionReason);
    }

    internal static Result<SessionReport> Create(
        TrainingSessionReportId id,
        TrainingSessionId sessionId,
        Guid operationId,
        string requestFingerprint,
        DateTimeOffset actualEndAtUtc,
        int grossDurationMinutes,
        int interruptionDurationMinutes,
        int deliveredDurationMinutes,
        decimal? distanceKilometers,
        string summary,
        string? objectivesWorked,
        string? objectivesAchieved,
        string? nextObjective,
        string? instructorComments,
        UserId completedByUserId,
        DateTimeOffset completedAtUtc)
    {
        if (id.IsEmpty || sessionId.IsEmpty || operationId == Guid.Empty || string.IsNullOrWhiteSpace(requestFingerprint) || completedByUserId.IsEmpty)
            return Result.Failure<SessionReport>(TrainingSessionErrors.CompletionInvalid);
        if (grossDurationMinutes <= 0 || interruptionDurationMinutes < 0 || deliveredDurationMinutes <= 0 || deliveredDurationMinutes > grossDurationMinutes)
            return Result.Failure<SessionReport>(TrainingSessionErrors.CompletionDurationInvalid);
        if (distanceKilometers.HasValue && (distanceKilometers.Value < 0 || distanceKilometers.Value > 5000))
            return Result.Failure<SessionReport>(TrainingSessionErrors.CompletionDistanceInvalid);

        string? normalizedSummary = Normalize(summary, 5000);
        if (normalizedSummary is null) return Result.Failure<SessionReport>(TrainingSessionErrors.CompletionSummaryInvalid);
        string? worked = Normalize(objectivesWorked, 4000);
        string? achieved = Normalize(objectivesAchieved, 4000);
        string? next = Normalize(nextObjective, 2000);
        string? comments = Normalize(instructorComments, 5000);
        if (!string.IsNullOrWhiteSpace(objectivesWorked) && worked is null || !string.IsNullOrWhiteSpace(objectivesAchieved) && achieved is null || !string.IsNullOrWhiteSpace(nextObjective) && next is null || !string.IsNullOrWhiteSpace(instructorComments) && comments is null)
            return Result.Failure<SessionReport>(TrainingSessionErrors.CompletionTextTooLong);

        DateTimeOffset now = completedAtUtc.ToUniversalTime();
        return Result.Success(new SessionReport(id)
        {
            TrainingSessionId = sessionId,
            OperationId = operationId,
            RequestFingerprint = requestFingerprint,
            Status = SessionReportStatus.Submitted,
            Version = 1,
            LastCompletedStep = 9,
            ActualEndAtUtc = actualEndAtUtc.ToUniversalTime(),
            GrossDurationMinutes = grossDurationMinutes,
            InterruptionDurationMinutes = interruptionDurationMinutes,
            DeliveredDurationMinutes = deliveredDurationMinutes,
            DistanceKilometers = distanceKilometers,
            Summary = normalizedSummary,
            ObjectivesWorked = worked,
            ObjectivesAchieved = achieved,
            NextObjective = next,
            InternalNote = comments,
            InstructorComments = comments,
            LastSavedByUserId = completedByUserId,
            LastSavedAtUtc = now,
            CompletedByUserId = completedByUserId,
            CompletedAtUtc = now
        });
    }

    private static Result ValidateDraft(int step, params string?[] texts)
    {
        if (step is < 1 or > 9) return Result.Failure(TrainingSessionErrors.ReportDraftStepInvalid);
        int[] limits = [5000, 4000, 4000, 2000, 5000, 5000];
        for (int i = 0; i < texts.Length; i++)
            if (!string.IsNullOrWhiteSpace(texts[i]) && texts[i]!.Trim().Length > limits[i])
                return Result.Failure(TrainingSessionErrors.ReportDraftTextTooLong);
        return Result.Success();
    }

    private static string? Normalize(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        string normalized = value.Trim();
        return normalized.Length <= maxLength ? normalized : null;
    }

    private static string BuildFingerprint(int step, params string?[] values)
    {
        string raw = string.Join('|', new[] { step.ToString() }.Concat(values.Select(x => x?.Trim() ?? string.Empty)));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));
    }
}
