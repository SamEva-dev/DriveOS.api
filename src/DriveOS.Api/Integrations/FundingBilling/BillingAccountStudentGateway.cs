using DriveOS.Modules.FundingBilling.Application.BillingAccounts.Create;
using DriveOS.Modules.Students.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Api.Integrations.FundingBilling;
public sealed class BillingAccountStudentGateway(StudentsDbContext students) : IBillingAccountStudentGateway
{
    public Task<bool> ExistsAsync(OrganizationId organizationId, PersonId studentId, CancellationToken ct = default) => students.Students.AsNoTracking().AnyAsync(x => x.OrganizationId == organizationId && x.Id == studentId, ct);
}
