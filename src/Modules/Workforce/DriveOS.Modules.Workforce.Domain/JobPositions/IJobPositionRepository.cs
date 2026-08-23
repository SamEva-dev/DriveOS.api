using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.Workforce.Domain.JobPositions;
public interface IJobPositionRepository
{
    Task<JobPosition?> GetByIdAsync(OrganizationId organizationId, JobPositionId id, CancellationToken cancellationToken = default);
    Task<JobPosition?> GetByIdForUpdateAsync(OrganizationId organizationId, JobPositionId id, CancellationToken cancellationToken = default);
    Task<JobPosition?> FindByCodeAsync(OrganizationId organizationId, string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<JobPosition>> ListAsync(OrganizationId organizationId, JobPositionStatus? status, CancellationToken cancellationToken = default);
    void Add(JobPosition position);
}
