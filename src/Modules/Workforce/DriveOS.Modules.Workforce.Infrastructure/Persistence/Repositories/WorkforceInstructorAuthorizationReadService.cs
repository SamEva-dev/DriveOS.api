using DriveOS.Modules.Workforce.Application.Qualifications;
using DriveOS.Modules.Workforce.Domain.Employees;
using DriveOS.Modules.Workforce.Domain.Qualifications;
using DriveOS.Modules.Workforce.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
namespace DriveOS.Modules.Workforce.Infrastructure.Persistence.Repositories;
internal sealed class WorkforceInstructorAuthorizationReadService(WorkforceDbContext db) : IWorkforceInstructorAuthorizationReadService
{
    public async Task<InstructorAuthorizationSnapshot?> ResolveCurrentAsync(OrganizationId org, UserId userId, string countryCode, string authorizationType, string? licenseCategoryCode, DateOnly atDate, CancellationToken ct=default)
    {
        string country=EmployeeQualification.NormalizeToken(countryCode), type=EmployeeQualification.NormalizeToken(authorizationType), category=EmployeeQualification.NormalizeToken(licenseCategoryCode??string.Empty);
        Employee? e=await db.Employees.AsNoTracking().Include(x=>x.InstructorAuthorizations).Where(x=>x.EmployerOrganizationId==org && x.UserId==userId && x.Status!=EmploymentStatus.Ended).OrderByDescending(x=>x.EmploymentStartDate).FirstOrDefaultAsync(ct);
        if(e is null)return null;
        var a=e.InstructorAuthorizations.Where(x=>x.CountryCode==country && x.AuthorizationType==type && (string.IsNullOrEmpty(category)||x.LicenseCategoryCode==category) && x.Status is EmployeeQualificationStatus.Declared or EmployeeQualificationStatus.Verified && (!x.IssuedOn.HasValue||x.IssuedOn.Value<=atDate) && (!x.ExpiresOn.HasValue||x.ExpiresOn.Value>=atDate)).OrderByDescending(x=>x.IssuedOn).FirstOrDefault();
        return a is null?null:new InstructorAuthorizationSnapshot(e.Id.Value,e.UserId?.Value,a.CountryCode,a.AuthorizationType,a.Identifier,a.IssuingAuthority,a.JurisdictionCode,a.LicenseCategoryCode,a.IssuedOn,a.ExpiresOn,a.Status==EmployeeQualificationStatus.Verified);
    }
}
