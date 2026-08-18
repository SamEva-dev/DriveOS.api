using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.FundingBilling.Application.BillingAccounts.Create;
public sealed record CreateBillingAccountCommand(OrganizationId OrganizationId, PersonId StudentId, string Currency, UserId ActorUserId) : ICommand<BillingAccountId>;
public interface IBillingAccountStudentGateway
{
    Task<bool> ExistsAsync(OrganizationId organizationId, PersonId studentId, CancellationToken cancellationToken = default);
}
public static class CreateBillingAccountErrors
{
    public static readonly Error StudentNotFound = Error.NotFound("FundingBilling.BillingAccount.Student.NotFound", "errors.fundingBilling.billingAccount.student.notFound");
}
