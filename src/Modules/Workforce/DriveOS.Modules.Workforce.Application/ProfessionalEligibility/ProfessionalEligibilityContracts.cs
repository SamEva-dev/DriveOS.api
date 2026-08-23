using DriveOS.Modules.Workforce.Domain.ProfessionalRestrictions;using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.Workforce.Application.ProfessionalEligibility;
public sealed record ProfessionalEligibilityResult(bool IsEligible,string? ReasonCode,Guid? RestrictionId);
public interface IWorkforceProfessionalEligibilityReadService
{
 Task<ProfessionalEligibilityResult> CheckAsync(OrganizationId organizationId,UserId userId,ProfessionalRestrictionActivity activity,DateOnly date,string? countryCode=null,string? licenseCategoryCode=null,BranchId? branchId=null,CancellationToken cancellationToken=default);
}
