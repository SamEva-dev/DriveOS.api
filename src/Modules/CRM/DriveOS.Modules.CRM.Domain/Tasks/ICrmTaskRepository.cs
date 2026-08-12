using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Domain.Tasks;

public interface ICrmTaskRepository
{
    void Add(CrmTask task);
    Task<CrmTask?> GetByIdForUpdateAsync(OrganizationId organizationId, CrmTaskId taskId, CancellationToken cancellationToken);
    Task<IReadOnlyList<CrmTask>> GetByLeadAsync(OrganizationId organizationId, LeadId leadId, CancellationToken cancellationToken);
    Task<IReadOnlyList<CrmTask>> GetPendingAsync(OrganizationId organizationId, CancellationToken cancellationToken);
}
