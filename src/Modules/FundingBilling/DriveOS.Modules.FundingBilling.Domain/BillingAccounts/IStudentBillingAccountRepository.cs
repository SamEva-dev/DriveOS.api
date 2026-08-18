using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Domain.BillingAccounts;

public interface IStudentBillingAccountRepository
{
    Task<BillingAccount?> GetByIdAsync(
        BillingAccountId billingAccountId,
        CancellationToken cancellationToken = default);

    Task<BillingAccount?> GetByStudentAsync(
        OrganizationId organizationId,
        PersonId studentId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        BillingAccount billingAccount,
        CancellationToken cancellationToken = default);
}
