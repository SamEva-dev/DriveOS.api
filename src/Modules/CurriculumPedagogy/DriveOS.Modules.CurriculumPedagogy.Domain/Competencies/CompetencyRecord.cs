using DriveOS.Modules.CurriculumPedagogy.Domain.Competencies.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CurriculumPedagogy.Domain.Competencies;

public sealed class CompetencyRecord : AggregateRoot<CompetencyRecordId>, IAuditableEntity
{
    private readonly List<CompetencyAssessment> _assessments = [];

    private CompetencyRecord() { }

    private CompetencyRecord(
        CompetencyRecordId id,
        OrganizationId organizationId,
        TrainingPathId trainingPathId,
        CurriculumVersionId curriculumVersionId,
        CompetencyId competencyId,
        bool isRequired)
        : base(id)
    {
        OrganizationId = organizationId;
        TrainingPathId = trainingPathId;
        CurriculumVersionId = curriculumVersionId;
        CompetencyId = competencyId;
        IsRequired = isRequired;
    }

    public OrganizationId OrganizationId { get; private set; }
    public TrainingPathId TrainingPathId { get; private set; }
    public CurriculumVersionId CurriculumVersionId { get; private set; }
    public CompetencyId CompetencyId { get; private set; }
    public bool IsRequired { get; private set; }
    public IReadOnlyCollection<CompetencyAssessment> Assessments => _assessments.AsReadOnly();

    // Never persisted as an independent source of truth. It is derived from assessment history.
    public string? CurrentLevelCode => CurrentAssessment?.LevelCode;
    public DateTimeOffset? LastAssessedAtUtc => CurrentAssessment?.AssessedAtUtc;
    public UserId? LastAssessorUserId => CurrentAssessment?.AssessorUserId;

    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    private CompetencyAssessment? CurrentAssessment => _assessments
        .OrderByDescending(x => x.AssessedAtUtc)
        .ThenByDescending(x => x.RecordedAtUtc)
        .FirstOrDefault();

    public static Result<CompetencyRecord> Create(
        CompetencyRecordId id,
        OrganizationId organizationId,
        TrainingPathId trainingPathId,
        CurriculumVersionId curriculumVersionId,
        CompetencyId competencyId,
        bool isRequired)
    {
        if (id.IsEmpty)
            return Result.Failure<CompetencyRecord>(CompetencyRecordErrors.InvalidIdentifier);
        if (organizationId.IsEmpty)
            return Result.Failure<CompetencyRecord>(CompetencyRecordErrors.InvalidOrganization);
        if (trainingPathId.IsEmpty)
            return Result.Failure<CompetencyRecord>(CompetencyRecordErrors.InvalidTrainingPath);
        if (curriculumVersionId.IsEmpty)
            return Result.Failure<CompetencyRecord>(CompetencyRecordErrors.InvalidCurriculumVersion);
        if (competencyId.IsEmpty)
            return Result.Failure<CompetencyRecord>(CompetencyRecordErrors.InvalidCompetency);

        var record = new CompetencyRecord(
            id, organizationId, trainingPathId, curriculumVersionId, competencyId, isRequired);

        record.RaiseDomainEvent(new CompetencyRecordCreatedDomainEvent(
            record.Id,
            record.OrganizationId,
            record.TrainingPathId,
            record.CurriculumVersionId,
            record.CompetencyId));

        return Result.Success(record);
    }

    public Result<CompetencyAssessment> RecordAssessment(
        CompetencyAssessmentId assessmentId,
        string levelCode,
        UserId assessorUserId,
        Guid? sourceSessionId,
        string? comment,
        bool isVisibleToStudent,
        DateTimeOffset assessedAtUtc,
        DateTimeOffset recordedAtUtc)
    {
        string? previousCurrentLevel = CurrentLevelCode;

        Result<CompetencyAssessment> assessmentResult = CompetencyAssessment.Create(
            assessmentId,
            Id,
            levelCode,
            assessorUserId,
            sourceSessionId,
            comment,
            isVisibleToStudent,
            assessedAtUtc,
            recordedAtUtc);

        if (assessmentResult.IsFailure)
            return assessmentResult;

        CompetencyAssessment assessment = assessmentResult.Value;
        _assessments.Add(assessment);
        SetModifiedAudit(recordedAtUtc, assessorUserId);

        RaiseDomainEvent(new CompetencyAssessedDomainEvent(
            Id,
            assessment.Id,
            TrainingPathId,
            CompetencyId,
            assessment.LevelCode,
            assessment.AssessorUserId,
            assessment.SourceSessionId,
            assessment.AssessedAtUtc));

        // A back-filled historical assessment must not rewrite the current level.
        string? currentLevel = CurrentLevelCode;
        if (!string.Equals(previousCurrentLevel, currentLevel, StringComparison.OrdinalIgnoreCase) && currentLevel is not null)
        {
            RaiseDomainEvent(new CompetencyLevelChangedDomainEvent(
                Id,
                TrainingPathId,
                CompetencyId,
                previousCurrentLevel,
                currentLevel,
                LastAssessedAtUtc!.Value));
        }

        return Result.Success(assessment);
    }

    public void SetCreatedAudit(DateTimeOffset createdAtUtc, UserId? createdByUserId)
    {
        if (CreatedAtUtc != default)
            return;
        CreatedAtUtc = createdAtUtc.ToUniversalTime();
        CreatedByUserId = createdByUserId;
    }

    public void SetModifiedAudit(DateTimeOffset modifiedAtUtc, UserId? modifiedByUserId)
    {
        LastModifiedAtUtc = modifiedAtUtc.ToUniversalTime();
        LastModifiedByUserId = modifiedByUserId;
    }
}
