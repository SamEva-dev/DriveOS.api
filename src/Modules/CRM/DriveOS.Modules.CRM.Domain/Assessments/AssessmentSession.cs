using System.Text.Json;
using DriveOS.Modules.CRM.Domain.Assessments.Events;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Domain.Assessments;

public sealed class AssessmentSession : AggregateRoot<AssessmentSessionId>, IAuditableEntity
{
    private AssessmentSession() { }

    private AssessmentSession(
        AssessmentSessionId id,
        OrganizationId organizationId,
        AssessmentAppointmentId appointmentId,
        LeadId leadId,
        UserId evaluatorUserId,
        string questionnaireCode,
        int questionnaireVersion,
        string questionnaireSnapshotJson,
        DateTimeOffset startedAtUtc
    )
        : base(id)
    {
        OrganizationId = organizationId;
        AppointmentId = appointmentId;
        LeadId = leadId;
        EvaluatorUserId = evaluatorUserId;
        QuestionnaireCode = questionnaireCode;
        QuestionnaireVersion = questionnaireVersion;
        QuestionnaireSnapshotJson = questionnaireSnapshotJson;
        AnswersJson = "[]";
        Status = AssessmentSessionStatus.InProgress;
        Revision = 1;
        StartedAtUtc = startedAtUtc.ToUniversalTime();
    }

    public OrganizationId OrganizationId { get; private set; }
    public AssessmentAppointmentId AppointmentId { get; private set; }
    public LeadId LeadId { get; private set; }
    public UserId EvaluatorUserId { get; private set; }
    public string QuestionnaireCode { get; private set; } = string.Empty;
    public int QuestionnaireVersion { get; private set; }
    public string QuestionnaireSnapshotJson { get; private set; } = "{}";
    public string AnswersJson { get; private set; } = "[]";
    public string? FactualObservations { get; private set; }
    public string? PedagogicalInterpretation { get; private set; }
    public string? Recommendation { get; private set; }
    public string? InternalNotes { get; private set; }
    public string? ProspectComment { get; private set; }
    public AssessmentSessionStatus Status { get; private set; }
    public int Revision { get; private set; }
    public DateTimeOffset StartedAtUtc { get; private set; }
    public DateTimeOffset? LastSavedAtUtc { get; private set; }
    public DateTimeOffset? SubmittedAtUtc { get; private set; }
    public UserId? SubmittedByUserId { get; private set; }
    public string? ResultJson { get; private set; }
    public string? AiSuggestionJson { get; private set; }
    public AssessmentResultConfidence? ResultConfidence { get; private set; }
    public AssessmentResultStatus ResultStatus { get; private set; }
    public string? CorrectionReason { get; private set; }
    public DateTimeOffset? ResultValidatedAtUtc { get; private set; }
    public UserId? ResultValidatedByUserId { get; private set; }
    public DateTimeOffset? ResultSharedAtUtc { get; private set; }
    public UserId? ResultSharedByUserId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<AssessmentSession> Start(
        AssessmentSessionId id,
        OrganizationId organizationId,
        AssessmentAppointmentId appointmentId,
        LeadId leadId,
        UserId evaluatorUserId,
        string questionnaireCode,
        int questionnaireVersion,
        string questionnaireSnapshotJson,
        DateTimeOffset startedAtUtc
    )
    {
        if (
            id == AssessmentSessionId.Empty
            || appointmentId == AssessmentAppointmentId.Empty
            || evaluatorUserId == UserId.Empty
        )
            return Result.Failure<AssessmentSession>(AssessmentSessionErrors.InvalidIdentifier);
        if (
            string.IsNullOrWhiteSpace(questionnaireCode)
            || questionnaireVersion <= 0
            || !IsJson(questionnaireSnapshotJson)
        )
            return Result.Failure<AssessmentSession>(AssessmentSessionErrors.InvalidQuestionnaire);

        var session = new AssessmentSession(
            id,
            organizationId,
            appointmentId,
            leadId,
            evaluatorUserId,
            questionnaireCode.Trim(),
            questionnaireVersion,
            questionnaireSnapshotJson,
            startedAtUtc
        );
        session.RaiseDomainEvent(
            new InitialAssessmentStartedDomainEvent(
                id,
                appointmentId,
                organizationId,
                evaluatorUserId,
                session.StartedAtUtc
            )
        );
        return Result.Success(session);
    }

    public Result SaveDraft(
        string answersJson,
        string? factualObservations,
        string? pedagogicalInterpretation,
        string? recommendation,
        string? internalNotes,
        string? prospectComment,
        bool draftCompleted,
        DateTimeOffset savedAtUtc
    )
    {
        if (Status == AssessmentSessionStatus.Submitted)
            return Result.Failure(AssessmentSessionErrors.AlreadySubmitted);
        if (!IsJsonArray(answersJson))
            return Result.Failure(AssessmentSessionErrors.InvalidAnswers);
        if (
            TooLong(factualObservations, 8000)
            || TooLong(pedagogicalInterpretation, 8000)
            || TooLong(recommendation, 8000)
            || TooLong(internalNotes, 8000)
            || TooLong(prospectComment, 4000)
        )
            return Result.Failure(AssessmentSessionErrors.NotesTooLong);

        AnswersJson = answersJson;
        FactualObservations = Normalize(factualObservations);
        PedagogicalInterpretation = Normalize(pedagogicalInterpretation);
        Recommendation = Normalize(recommendation);
        InternalNotes = Normalize(internalNotes);
        ProspectComment = Normalize(prospectComment);
        Status = draftCompleted
            ? AssessmentSessionStatus.DraftCompleted
            : AssessmentSessionStatus.InProgress;
        Revision++;
        LastSavedAtUtc = savedAtUtc.ToUniversalTime();
        RaiseDomainEvent(
            new InitialAssessmentDraftSavedDomainEvent(
                Id,
                OrganizationId,
                Revision,
                LastSavedAtUtc.Value
            )
        );
        return Result.Success();
    }

    public Result Submit(UserId submittedByUserId, DateTimeOffset submittedAtUtc)
    {
        if (Status == AssessmentSessionStatus.Submitted)
            return Result.Failure(AssessmentSessionErrors.AlreadySubmitted);
        if (AnswersJson == "[]")
            return Result.Failure(AssessmentSessionErrors.SubmissionRequiresAnswers);
        Status = AssessmentSessionStatus.Submitted;
        Revision++;
        SubmittedAtUtc = submittedAtUtc.ToUniversalTime();
        SubmittedByUserId = submittedByUserId;
        RaiseDomainEvent(
            new InitialAssessmentSubmittedDomainEvent(
                Id,
                AppointmentId,
                OrganizationId,
                submittedByUserId,
                Revision,
                SubmittedAtUtc.Value
            )
        );
        return Result.Success();
    }

    public Result SaveResult(
        string resultJson,
        AssessmentResultConfidence confidence,
        string? aiSuggestionJson,
        UserId savedByUserId,
        DateTimeOffset savedAtUtc
    )
    {
        if (Status != AssessmentSessionStatus.Submitted)
            return Result.Failure(AssessmentSessionErrors.ResultRequiresSubmittedAssessment);
        if (
            !IsJsonObject(resultJson)
            || (aiSuggestionJson is not null && !IsJsonObject(aiSuggestionJson))
        )
            return Result.Failure(AssessmentSessionErrors.InvalidResult);
        if (ResultStatus is AssessmentResultStatus.Validated or AssessmentResultStatus.Shared)
            return Result.Failure(AssessmentSessionErrors.ValidatedResultIsImmutable);

        ResultJson = resultJson;
        AiSuggestionJson = Normalize(aiSuggestionJson);
        ResultConfidence = confidence;
        ResultStatus = AssessmentResultStatus.Draft;
        CorrectionReason = null;
        Revision++;
        LastSavedAtUtc = savedAtUtc.ToUniversalTime();
        RaiseDomainEvent(
            new InitialAssessmentResultDraftSavedDomainEvent(
                Id,
                OrganizationId,
                savedByUserId,
                Revision,
                LastSavedAtUtc.Value
            )
        );
        return Result.Success();
    }

    public Result RequestResultCorrection(
        string reason,
        UserId requestedByUserId,
        DateTimeOffset requestedAtUtc
    )
    {
        if (ResultStatus != AssessmentResultStatus.Draft || string.IsNullOrWhiteSpace(ResultJson))
            return Result.Failure(AssessmentSessionErrors.ResultNotReady);
        if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length > 2000)
            return Result.Failure(AssessmentSessionErrors.InvalidCorrectionReason);

        ResultStatus = AssessmentResultStatus.CorrectionRequested;
        CorrectionReason = reason.Trim();
        Revision++;
        LastSavedAtUtc = requestedAtUtc.ToUniversalTime();
        RaiseDomainEvent(
            new InitialAssessmentResultCorrectionRequestedDomainEvent(
                Id,
                OrganizationId,
                requestedByUserId,
                Revision,
                LastSavedAtUtc.Value
            )
        );
        return Result.Success();
    }

    public Result ValidateResult(UserId validatedByUserId, DateTimeOffset validatedAtUtc)
    {
        if (ResultStatus != AssessmentResultStatus.Draft || string.IsNullOrWhiteSpace(ResultJson))
            return Result.Failure(AssessmentSessionErrors.ResultNotReady);

        ResultStatus = AssessmentResultStatus.Validated;
        CorrectionReason = null;
        ResultValidatedAtUtc = validatedAtUtc.ToUniversalTime();
        ResultValidatedByUserId = validatedByUserId;
        Revision++;
        RaiseDomainEvent(
            new InitialAssessmentResultValidatedDomainEvent(
                Id,
                AppointmentId,
                LeadId,
                OrganizationId,
                validatedByUserId,
                Revision,
                ResultValidatedAtUtc.Value
            )
        );
        return Result.Success();
    }

    public Result MarkResultShared(UserId sharedByUserId, DateTimeOffset sharedAtUtc)
    {
        if (ResultStatus != AssessmentResultStatus.Validated)
            return Result.Failure(AssessmentSessionErrors.ResultMustBeValidated);

        ResultStatus = AssessmentResultStatus.Shared;
        ResultSharedAtUtc = sharedAtUtc.ToUniversalTime();
        ResultSharedByUserId = sharedByUserId;
        Revision++;
        RaiseDomainEvent(
            new InitialAssessmentResultSharedDomainEvent(
                Id,
                LeadId,
                OrganizationId,
                sharedByUserId,
                Revision,
                ResultSharedAtUtc.Value
            )
        );
        return Result.Success();
    }

    public void SetCreatedAudit(DateTimeOffset at, UserId? by)
    {
        if (CreatedAtUtc == default)
        {
            CreatedAtUtc = at;
            CreatedByUserId = by;
        }
    }

    public void SetModifiedAudit(DateTimeOffset at, UserId? by)
    {
        LastModifiedAtUtc = at;
        LastModifiedByUserId = by;
    }

    private static bool IsJson(string value)
    {
        try
        {
            using JsonDocument _ = JsonDocument.Parse(value);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool IsJsonArray(string value)
    {
        try
        {
            using JsonDocument doc = JsonDocument.Parse(value);
            return doc.RootElement.ValueKind == JsonValueKind.Array;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool IsJsonObject(string value)
    {
        try
        {
            using JsonDocument doc = JsonDocument.Parse(value);
            return doc.RootElement.ValueKind == JsonValueKind.Object;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool TooLong(string? value, int max) => value?.Trim().Length > max;

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
