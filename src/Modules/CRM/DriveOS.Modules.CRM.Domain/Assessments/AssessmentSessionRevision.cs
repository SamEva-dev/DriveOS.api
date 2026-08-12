using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Domain.Assessments;

public sealed class AssessmentSessionRevision : Entity<AssessmentSessionRevisionId>
{
    private AssessmentSessionRevision() { }
    private AssessmentSessionRevision(AssessmentSessionRevisionId id, AssessmentSession session, UserId savedByUserId, DateTimeOffset savedAtUtc) : base(id)
    {
        OrganizationId = session.OrganizationId; SessionId = session.Id; Revision = session.Revision;
        AnswersJson = session.AnswersJson; FactualObservations = session.FactualObservations;
        PedagogicalInterpretation = session.PedagogicalInterpretation; Recommendation = session.Recommendation;
        InternalNotes = session.InternalNotes; ProspectComment = session.ProspectComment;
        ResultJson = session.ResultJson; AiSuggestionJson = session.AiSuggestionJson;
        ResultConfidence = session.ResultConfidence; ResultStatus = session.ResultStatus;
        CorrectionReason = session.CorrectionReason;
        Status = session.Status; SavedByUserId = savedByUserId; SavedAtUtc = savedAtUtc.ToUniversalTime();
    }
    public OrganizationId OrganizationId { get; private set; }
    public AssessmentSessionId SessionId { get; private set; }
    public int Revision { get; private set; }
    public string AnswersJson { get; private set; } = "[]";
    public string? FactualObservations { get; private set; }
    public string? PedagogicalInterpretation { get; private set; }
    public string? Recommendation { get; private set; }
    public string? InternalNotes { get; private set; }
    public string? ProspectComment { get; private set; }
    public string? ResultJson { get; private set; }
    public string? AiSuggestionJson { get; private set; }
    public AssessmentResultConfidence? ResultConfidence { get; private set; }
    public AssessmentResultStatus ResultStatus { get; private set; }
    public string? CorrectionReason { get; private set; }
    public AssessmentSessionStatus Status { get; private set; }
    public UserId SavedByUserId { get; private set; }
    public DateTimeOffset SavedAtUtc { get; private set; }
    public static AssessmentSessionRevision Capture(AssessmentSession session, UserId savedByUserId, DateTimeOffset savedAtUtc) =>
        new(AssessmentSessionRevisionId.New(), session, savedByUserId, savedAtUtc);
}
