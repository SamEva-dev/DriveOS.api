using DriveOS.Modules.ProfessionalMarketplace.Application.StudentAssignments;
using DriveOS.Modules.Students.Application.Students.Identity;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Api.Integrations.ProfessionalMarketplace;

internal sealed class ProfessionalStudentScopeGateway(
    IStudentIdentityService students):IProfessionalStudentScopeGateway
{
    public async Task<bool> ExistsAsync(
        OrganizationId organizationId,
        PersonId studentId,
        CancellationToken ct=default)=>
        await students.GetAsync(organizationId,studentId,ct) is not null;

    public async Task<ProfessionalStudentScopeStudent?> GetAsync(
        OrganizationId organizationId,
        PersonId studentId,
        CancellationToken ct=default)
    {
        StudentIdentityResponse? student=await students.GetAsync(organizationId,studentId,ct);
        if(student is null)return null;
        string displayName=string.Join(' ',new[]{student.LegalFirstName,student.LegalLastName}.Where(x=>!string.IsNullOrWhiteSpace(x))).Trim();
        return new ProfessionalStudentScopeStudent(student.StudentId,displayName,student.Email,student.Phone);
    }
}
