using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CurriculumPedagogy.Application.Competencies;

public sealed record RecordCompetencyAssessmentCommand(
    OrganizationId OrganizationId,
    TrainingPathId TrainingPathId,
    CompetencyId CompetencyId,
    string LevelCode,
    UserId AssessorUserId,
    Guid? SourceSessionId,
    string? Comment,
    bool IsVisibleToStudent,
    DateTimeOffset? AssessedAtUtc) : ICommand<CompetencyAssessmentId>;

public sealed record CurriculumCompetencyEligibility(
    CurriculumVersionId CurriculumVersionId,
    CompetencyId CompetencyId,
    string Code,
    string Name,
    bool IsRequired);

public interface ICurriculumCompetencyEligibilityService
{
    Task<CurriculumCompetencyEligibility?> GetAsync(
        OrganizationId organizationId,
        CurriculumVersionId curriculumVersionId,
        CompetencyId competencyId,
        CancellationToken cancellationToken = default);
}

public static class RecordCompetencyAssessmentErrors
{
    public static readonly Error TrainingPathNotFound = Error.NotFound(
        "CurriculumPedagogy.CompetencyAssessment.TrainingPath.NotFound",
        "errors.curriculumPedagogy.competencyAssessment.trainingPath.notFound");

    public static readonly Error TrainingPathNotActive = Error.Conflict(
        "CurriculumPedagogy.CompetencyAssessment.TrainingPath.NotActive",
        "errors.curriculumPedagogy.competencyAssessment.trainingPath.notActive");

    public static readonly Error CompetencyNotFound = Error.NotFound(
        "CurriculumPedagogy.CompetencyAssessment.Competency.NotFound",
        "errors.curriculumPedagogy.competencyAssessment.competency.notFound");
}

public sealed record CompetencyAssessmentResponse(
    Guid Id,
    string LevelCode,
    Guid AssessorUserId,
    Guid? SourceSessionId,
    string? Comment,
    bool IsVisibleToStudent,
    DateTimeOffset AssessedAtUtc,
    DateTimeOffset RecordedAtUtc);

public sealed record CompetencyRecordResponse(
    Guid? Id,
    Guid TrainingPathId,
    Guid CurriculumVersionId,
    Guid CompetencyId,
    string CompetencyCode,
    string CompetencyName,
    bool IsRequired,
    string? CurrentLevelCode,
    DateTimeOffset? LastAssessedAtUtc,
    Guid? LastAssessorUserId,
    IReadOnlyCollection<CompetencyAssessmentResponse> Assessments);

public interface ICompetencyRecordReadService
{
    Task<IReadOnlyCollection<CompetencyRecordResponse>> ListForTrainingPathAsync(
        OrganizationId organizationId,
        TrainingPathId trainingPathId,
        bool includeInternalComments,
        CancellationToken cancellationToken = default);

    Task<CompetencyRecordResponse?> GetAsync(
        OrganizationId organizationId,
        TrainingPathId trainingPathId,
        CompetencyId competencyId,
        bool includeInternalComments,
        CancellationToken cancellationToken = default);
}
