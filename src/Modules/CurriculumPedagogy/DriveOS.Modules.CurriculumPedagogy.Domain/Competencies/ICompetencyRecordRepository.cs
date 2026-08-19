using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CurriculumPedagogy.Domain.Competencies;

public interface ICompetencyRecordRepository
{
    Task<CompetencyRecord?> GetByIdAsync(
        OrganizationId organizationId,
        CompetencyRecordId competencyRecordId,
        CancellationToken cancellationToken = default);

    Task<CompetencyRecord?> GetByIdForUpdateAsync(
        OrganizationId organizationId,
        CompetencyRecordId competencyRecordId,
        CancellationToken cancellationToken = default);

    Task<CompetencyRecord?> GetByTrainingPathAndCompetencyAsync(
        OrganizationId organizationId,
        TrainingPathId trainingPathId,
        CompetencyId competencyId,
        CancellationToken cancellationToken = default);

    Task<CompetencyRecord?> GetByTrainingPathAndCompetencyForUpdateAsync(
        OrganizationId organizationId,
        TrainingPathId trainingPathId,
        CompetencyId competencyId,
        CancellationToken cancellationToken = default);

    Task AddAsync(CompetencyRecord competencyRecord, CancellationToken cancellationToken = default);
}
