using DriveOS.Modules.FundingBilling.Application.BillingAccounts.Create;
using DriveOS.Modules.Students.Application.References;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Api.Integrations.FundingBilling;
public sealed class BillingAccountStudentGateway(IStudentReferenceReadService students) : IBillingAccountStudentGateway
{
    public Task<bool> ExistsAsync(OrganizationId organizationId, PersonId studentId, CancellationToken ct = default) => students.ExistsAsync(organizationId, studentId, ct);
}
