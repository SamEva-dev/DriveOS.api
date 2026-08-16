using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Domain.Conversions;

public interface ILeadConversionRepository
{
    Task<LeadConversion?> GetByLeadIdAsync(
        OrganizationId organizationId,
        LeadId leadId,
        CancellationToken cancellationToken = default
    );
    Task<LeadConversion?> GetByLeadIdForUpdateAsync(
        OrganizationId organizationId,
        LeadId leadId,
        CancellationToken cancellationToken = default
    );
    Task AddAsync(LeadConversion conversion, CancellationToken cancellationToken = default);
}
