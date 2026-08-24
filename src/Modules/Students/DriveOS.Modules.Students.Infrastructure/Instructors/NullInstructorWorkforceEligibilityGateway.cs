using DriveOS.Modules.Students.Application.Instructors;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Students.Infrastructure.Instructors;

/// <summary>
/// Safe module-local fallback. The DriveOS API composition root replaces this implementation with
/// the Workforce-backed gateway when the complete application is hosted.
/// </summary>
internal sealed class NullInstructorWorkforceEligibilityGateway : IInstructorWorkforceEligibilityGateway
{
    public Task<InstructorWorkforceEligibility> VerifyAsync(
        OrganizationId organizationId,
        UserId instructorId,
        BranchId? branchId,
        string trainingCategory,
        DateOnly effectiveDate,
        CancellationToken ct = default)
        => Task.FromResult(new InstructorWorkforceEligibility(true, null));
}
