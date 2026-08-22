using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Domain.Places;

public interface IExamPlaceRepository
{
    Task<ExamPlace?> GetByIdAsync(OrganizationId organizationId, ExamPlaceId id, CancellationToken cancellationToken = default);
    Task<ExamPlace?> GetByIdForUpdateAsync(OrganizationId organizationId, ExamPlaceId id, CancellationToken cancellationToken = default);
    Task<ExamPlace?> FindByExternalIdAsync(OrganizationId organizationId, string providerCode, string externalPlaceId, CancellationToken cancellationToken = default);
    Task<ExamPlace?> FindByExternalIdForUpdateAsync(OrganizationId organizationId, string providerCode, string externalPlaceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExamPlace>> ListAvailableAsync(OrganizationId organizationId, DateTimeOffset fromUtc, DateTimeOffset toUtc, string? licenseCategory, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExamPlace>> ListExternalForUpdateAsync(OrganizationId organizationId, string providerCode, DateTimeOffset fromUtc, DateTimeOffset toUtc, string? examCategory, IReadOnlyCollection<string>? centerExternalIds, CancellationToken cancellationToken = default);
    void Add(ExamPlace place);
}
