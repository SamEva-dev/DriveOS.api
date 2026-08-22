using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Domain.Providers.Connections;

public interface IExamProviderConnectionRepository
{
    Task<ExamProviderConnection?> GetByIdAsync(OrganizationId organizationId, ExamProviderConnectionId id, CancellationToken cancellationToken = default);
    Task<ExamProviderConnection?> GetByIdForUpdateAsync(OrganizationId organizationId, ExamProviderConnectionId id, CancellationToken cancellationToken = default);
    Task<ExamProviderConnection?> FindByProviderCodeAsync(OrganizationId organizationId, string providerCode, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExamProviderConnection>> ListAsync(OrganizationId organizationId, CancellationToken cancellationToken = default);
    void Add(ExamProviderConnection connection);
}
