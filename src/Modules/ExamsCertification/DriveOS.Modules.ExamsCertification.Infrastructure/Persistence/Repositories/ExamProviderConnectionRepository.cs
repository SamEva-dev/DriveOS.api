using DriveOS.Modules.ExamsCertification.Domain.Providers.Connections;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Repositories;

internal sealed class ExamProviderConnectionRepository(ExamsCertificationDbContext dbContext) : IExamProviderConnectionRepository
{
    public Task<ExamProviderConnection?> GetByIdAsync(OrganizationId organizationId, ExamProviderConnectionId id,
        CancellationToken cancellationToken = default) => dbContext.ExamProviderConnections.AsNoTracking()
            .SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.Id == id, cancellationToken);

    public Task<ExamProviderConnection?> GetByIdForUpdateAsync(OrganizationId organizationId, ExamProviderConnectionId id,
        CancellationToken cancellationToken = default) => dbContext.ExamProviderConnections
            .SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.Id == id, cancellationToken);

    public Task<ExamProviderConnection?> FindByProviderCodeAsync(OrganizationId organizationId, string providerCode,
        CancellationToken cancellationToken = default) => dbContext.ExamProviderConnections.AsNoTracking()
            .SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.ProviderCode == providerCode.ToLower(), cancellationToken);

    public async Task<IReadOnlyList<ExamProviderConnection>> ListAsync(OrganizationId organizationId,
        CancellationToken cancellationToken = default) => await dbContext.ExamProviderConnections.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId)
            .OrderBy(x => x.DisplayName)
            .ToListAsync(cancellationToken);

    public void Add(ExamProviderConnection connection) => dbContext.ExamProviderConnections.Add(connection);
}
