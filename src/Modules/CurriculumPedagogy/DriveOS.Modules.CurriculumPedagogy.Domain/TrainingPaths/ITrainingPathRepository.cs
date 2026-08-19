using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CurriculumPedagogy.Domain.TrainingPaths;

public interface ITrainingPathRepository
{
    Task<TrainingPath?> GetByIdAsync(
        TrainingPathId trainingPathId,
        OrganizationId organizationId,
        CancellationToken cancellationToken = default);

    Task<TrainingPath?> GetByIdForUpdateAsync(
        TrainingPathId trainingPathId,
        OrganizationId organizationId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsOpenForStudentAndVersionAsync(
        OrganizationId organizationId,
        PersonId studentId,
        CurriculumVersionId curriculumVersionId,
        CancellationToken cancellationToken = default);

    Task AddAsync(TrainingPath trainingPath, CancellationToken cancellationToken = default);
}
