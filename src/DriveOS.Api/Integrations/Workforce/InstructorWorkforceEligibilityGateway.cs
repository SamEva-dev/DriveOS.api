using DriveOS.Modules.Students.Application.Instructors;
using DriveOS.Modules.Workforce.Application.ProfessionalEligibility;
using DriveOS.Modules.Workforce.Domain.ProfessionalRestrictions;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Api.Integrations.Workforce;

internal sealed class InstructorWorkforceEligibilityGateway(
    IWorkforceProfessionalEligibilityReadService workforce)
    : IInstructorWorkforceEligibilityGateway
{
    public async Task<InstructorWorkforceEligibility> VerifyAsync(
        OrganizationId organizationId,
        UserId instructorId,
        BranchId? branchId,
        string trainingCategory,
        DateOnly effectiveDate,
        CancellationToken ct = default)
    {
        ProfessionalEligibilityResult result = await workforce.CheckAsync(
            organizationId,
            instructorId,
            ProfessionalRestrictionActivity.Teaching,
            effectiveDate,
            countryCode: null,
            licenseCategoryCode: trainingCategory,
            branchId: branchId,
            cancellationToken: ct);

        return new InstructorWorkforceEligibility(result.IsEligible, result.ReasonCode);
    }
}
