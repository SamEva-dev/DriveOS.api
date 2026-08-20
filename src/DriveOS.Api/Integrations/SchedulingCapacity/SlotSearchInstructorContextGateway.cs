using DriveOS.Modules.SchedulingCapacity.Application.SlotSearch;
using DriveOS.Modules.Students.Application.Instructors;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Api.Integrations.SchedulingCapacity;

internal sealed class SlotSearchInstructorContextGateway(
    IInstructorEligibilityGateway eligibilityGateway,
    IStudentInstructorService studentInstructorService) : ISlotSearchInstructorContextGateway
{
    public async Task<SlotSearchInstructorContext> EvaluateAsync(
        OrganizationId organizationId,
        PersonId studentId,
        UserId instructorId,
        BranchId? branchId,
        string trainingCategory,
        CancellationToken cancellationToken = default)
    {
        InstructorEligibility eligibility = await eligibilityGateway.VerifyAsync(
            organizationId,
            instructorId,
            branchId,
            trainingCategory,
            cancellationToken);

        StudentInstructorsResponse? portfolio = await studentInstructorService.GetAsync(
            new GetStudentInstructorsQuery(organizationId, studentId),
            cancellationToken);

        bool continuity = portfolio is not null &&
            (portfolio.PrimaryInstructorId == instructorId.Value ||
             portfolio.Assignments.Any(x => x.InstructorId == instructorId.Value));

        return new SlotSearchInstructorContext(
            QualificationVerified: eligibility.IsEligible,
            IsEligible: eligibility.IsEligible,
            HasStudentContinuity: continuity,
            Warnings: eligibility.Warnings);
    }
}
