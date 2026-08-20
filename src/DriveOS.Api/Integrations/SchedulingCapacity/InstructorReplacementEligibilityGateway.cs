using DriveOS.Modules.SchedulingCapacity.Application.Replacements;
using DriveOS.Modules.Students.Application.Instructors;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Api.Integrations.SchedulingCapacity;

internal sealed class InstructorReplacementEligibilityGateway(
    IInstructorEligibilityGateway studentsEligibility,
    IStudentInstructorService studentInstructors) : IInstructorReplacementEligibilityGateway
{
    public async Task<InstructorReplacementEligibility> EvaluateAsync(
        OrganizationId organizationId,
        PersonId? studentId,
        UserId instructorId,
        BranchId? branchId,
        string trainingCategory,
        CancellationToken cancellationToken = default)
    {
        InstructorEligibility eligibility = await studentsEligibility.VerifyAsync(
            organizationId, instructorId, branchId, trainingCategory, cancellationToken);

        bool continuity = false;
        if (studentId.HasValue)
        {
            StudentInstructorsResponse? portfolio = await studentInstructors.GetAsync(
                new GetStudentInstructorsQuery(organizationId, studentId.Value), cancellationToken);
            continuity = portfolio is not null &&
                         (portfolio.Assignments.Any(x => x.InstructorId == instructorId.Value) ||
                          portfolio.History.Any(h => portfolio.Assignments.Any(a => a.Id == h.AssignmentId && a.InstructorId == instructorId.Value)));
        }

        return new InstructorReplacementEligibility(
            eligibility.IsEligible,
            eligibility.IsEligible,
            continuity,
            eligibility.Warnings);
    }
}
