using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Domain.Activities;

public interface ICrmActivityRepository
{
    void Add(CrmActivity activity);
    Task<IReadOnlyList<CrmActivity>> GetByLeadAsync(OrganizationId organizationId,
        LeadId leadId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CrmActivity>> GetRecentAsync(OrganizationId organizationId,
        int limit, CancellationToken cancellationToken = default);
    Task<CrmActivity?> GetByIdempotencyKeyAsync(OrganizationId organizationId,
        string idempotencyKey, CancellationToken cancellationToken = default);
}
