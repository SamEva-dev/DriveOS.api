using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CurriculumPedagogy.Application.Progression;

public sealed record PedagogicalProgressionLevelDistributionResponse(
    string LevelCode,
    int CompetencyCount,
    decimal Percentage);

public sealed record PedagogicalProgressionModuleResponse(
    Guid ModuleId,
    string ModuleCode,
    string ModuleName,
    int Order,
    int TotalCompetencies,
    int RequiredCompetencies,
    int AssessedCompetencies,
    int UnassessedCompetencies,
    int AssessmentCount,
    decimal AssessmentCoveragePercent,
    decimal RequiredAssessmentCoveragePercent,
    IReadOnlyCollection<PedagogicalProgressionLevelDistributionResponse> CurrentLevelDistribution);

public sealed record PedagogicalProgressionCompetencyResponse(
    Guid CompetencyId,
    Guid ModuleId,
    string ModuleCode,
    string CompetencyCode,
    string CompetencyName,
    bool IsRequired,
    int Order,
    string? CurrentLevelCode,
    DateTimeOffset? LastAssessedAtUtc,
    Guid? LastAssessorUserId,
    int AssessmentCount,
    int LevelChangeCount);

public sealed record PedagogicalProgressionTimelineItemResponse(
    Guid AssessmentId,
    Guid CompetencyId,
    string CompetencyCode,
    string CompetencyName,
    Guid ModuleId,
    string ModuleCode,
    string ModuleName,
    string LevelCode,
    string? PreviousLevelCode,
    bool LevelChanged,
    Guid AssessorUserId,
    Guid? SourceSessionId,
    string? Comment,
    bool IsVisibleToStudent,
    DateTimeOffset AssessedAtUtc,
    DateTimeOffset RecordedAtUtc);

public sealed record PedagogicalProgressionOverviewResponse(
    Guid TrainingPathId,
    Guid StudentId,
    Guid CurriculumVersionId,
    string TrainingPathStatus,
    int TotalModules,
    int TotalCompetencies,
    int RequiredCompetencies,
    int AssessedCompetencies,
    int AssessedRequiredCompetencies,
    int UnassessedCompetencies,
    int AssessmentCount,
    int LevelChangeCount,
    decimal AssessmentCoveragePercent,
    decimal RequiredAssessmentCoveragePercent,
    DateTimeOffset? FirstAssessmentAtUtc,
    DateTimeOffset? LastAssessmentAtUtc,
    IReadOnlyCollection<PedagogicalProgressionLevelDistributionResponse> CurrentLevelDistribution,
    IReadOnlyCollection<PedagogicalProgressionModuleResponse> Modules,
    IReadOnlyCollection<PedagogicalProgressionCompetencyResponse> Competencies,
    IReadOnlyCollection<PedagogicalProgressionTimelineItemResponse> RecentTimeline);

public interface IPedagogicalProgressionReadService
{
    Task<PedagogicalProgressionOverviewResponse?> GetOverviewAsync(
        OrganizationId organizationId,
        TrainingPathId trainingPathId,
        bool includeInternalComments,
        int recentTimelineLimit = 50,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<PedagogicalProgressionTimelineItemResponse>?> GetHistoryAsync(
        OrganizationId organizationId,
        TrainingPathId trainingPathId,
        bool includeInternalComments,
        CancellationToken cancellationToken = default);
}
