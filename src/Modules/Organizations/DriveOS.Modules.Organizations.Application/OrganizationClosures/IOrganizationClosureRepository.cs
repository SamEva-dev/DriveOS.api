using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Domain.OrganizationClosures;

public interface IOrganizationClosureRepository
{
    Task<OrganizationClosure?> GetForUpdateAsync(OrganizationClosureId closureId, CancellationToken cancellationToken = default);
    Task<OrganizationClosure?> GetByIdAsync(OrganizationClosureId closureId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrganizationClosure>> ListByOrganizationAsync(OrganizationId organizationId, CancellationToken cancellationToken = default);
    Task<OrganizationClosure?> GetOpenByOrganizationAsync(OrganizationId organizationId, CancellationToken cancellationToken = default);
    Task<bool> HasOpenClosureAsync(OrganizationId organizationId, CancellationToken cancellationToken = default);
    Task AddAsync(OrganizationClosure closure, CancellationToken cancellationToken = default);
}
