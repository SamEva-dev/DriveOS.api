using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.Workforce.Domain.LeavePolicies;
public interface ILeavePolicyRepository
{
 Task<LeavePolicy?> GetByIdAsync(OrganizationId organizationId,LeavePolicyId id,CancellationToken ct=default);
 Task<LeavePolicy?> GetByIdForUpdateAsync(OrganizationId organizationId,LeavePolicyId id,CancellationToken ct=default);
 Task<LeavePolicy?> FindByCodeAsync(OrganizationId organizationId,string countryCode,string code,CancellationToken ct=default);
 Task<IReadOnlyList<LeavePolicy>> ListAsync(OrganizationId organizationId,string? countryCode,LeavePolicyStatus? status,CancellationToken ct=default);
 void Add(LeavePolicy policy);
}
