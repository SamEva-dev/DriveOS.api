using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Domain.Places;

public interface IExamCenterRepository
{
    Task<ExamCenter?> GetByIdAsync(OrganizationId organizationId, ExamCenterId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExamCenter>> ListAsync(OrganizationId organizationId, CancellationToken cancellationToken = default);
    Task<ExamCenter?> FindByExternalIdAsync(OrganizationId organizationId, string providerCode, string externalCenterId, CancellationToken cancellationToken = default);
    Task<ExamCenter?> FindByExternalIdForUpdateAsync(OrganizationId organizationId, string providerCode, string externalCenterId, CancellationToken cancellationToken = default);
    void Add(ExamCenter center);
}
