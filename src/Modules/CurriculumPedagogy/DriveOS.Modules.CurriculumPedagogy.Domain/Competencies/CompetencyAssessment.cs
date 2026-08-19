using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CurriculumPedagogy.Domain.Competencies;

public sealed class CompetencyAssessment : Entity<CompetencyAssessmentId>
{
    private CompetencyAssessment() { }

    private CompetencyAssessment(
        CompetencyAssessmentId id,
        CompetencyRecordId competencyRecordId,
        string levelCode,
        UserId assessorUserId,
        Guid? sourceSessionId,
        string? comment,
        bool isVisibleToStudent,
        DateTimeOffset assessedAtUtc,
        DateTimeOffset recordedAtUtc)
        : base(id)
    {
        CompetencyRecordId = competencyRecordId;
        LevelCode = levelCode;
        AssessorUserId = assessorUserId;
        SourceSessionId = sourceSessionId;
        Comment = comment;
        IsVisibleToStudent = isVisibleToStudent;
        AssessedAtUtc = assessedAtUtc;
        RecordedAtUtc = recordedAtUtc;
    }

    public CompetencyRecordId CompetencyRecordId { get; private set; }
    public string LevelCode { get; private set; } = string.Empty;
    public UserId AssessorUserId { get; private set; }
    public Guid? SourceSessionId { get; private set; }
    public string? Comment { get; private set; }
    public bool IsVisibleToStudent { get; private set; }
    public DateTimeOffset AssessedAtUtc { get; private set; }
    public DateTimeOffset RecordedAtUtc { get; private set; }

    internal static Result<CompetencyAssessment> Create(
        CompetencyAssessmentId id,
        CompetencyRecordId competencyRecordId,
        string levelCode,
        UserId assessorUserId,
        Guid? sourceSessionId,
        string? comment,
        bool isVisibleToStudent,
        DateTimeOffset assessedAtUtc,
        DateTimeOffset recordedAtUtc)
    {
        if (id.IsEmpty || competencyRecordId.IsEmpty || assessorUserId.IsEmpty ||
            assessedAtUtc == default || recordedAtUtc == default)
        {
            return Result.Failure<CompetencyAssessment>(CompetencyRecordErrors.InvalidAssessment);
        }

        string normalizedLevel = (levelCode ?? string.Empty).Trim().ToUpperInvariant();
        if (normalizedLevel.Length is < 1 or > 60 || !normalizedLevel.All(IsCodeCharacter))
            return Result.Failure<CompetencyAssessment>(CompetencyRecordErrors.InvalidLevelCode);

        string? normalizedComment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        if (normalizedComment?.Length > 4000)
            return Result.Failure<CompetencyAssessment>(CompetencyRecordErrors.InvalidComment);

        return Result.Success(new CompetencyAssessment(
            id,
            competencyRecordId,
            normalizedLevel,
            assessorUserId,
            sourceSessionId,
            normalizedComment,
            isVisibleToStudent,
            assessedAtUtc.ToUniversalTime(),
            recordedAtUtc.ToUniversalTime()));
    }

    private static bool IsCodeCharacter(char value) =>
        char.IsLetterOrDigit(value) || value is '-' or '_' or '.';
}
