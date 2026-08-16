using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Domain.Leads;

public interface ILeadRepository
{
    Task<Lead?> GetByIdAsync(
        OrganizationId organizationId,
        LeadId id,
        CancellationToken cancellationToken = default
    );

    Task<Lead?> GetByIdForUpdateAsync(
        OrganizationId organizationId,
        LeadId id,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsByEmailAsync(
        OrganizationId organizationId,
        string email,
        CancellationToken cancellationToken = default
    );

    Task AddAsync(Lead lead, CancellationToken cancellationToken = default);
}
