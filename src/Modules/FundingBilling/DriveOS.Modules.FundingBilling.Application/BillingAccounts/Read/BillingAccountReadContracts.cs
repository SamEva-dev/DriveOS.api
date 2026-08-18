using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.FundingBilling.Application.BillingAccounts.Read;
public sealed record BillingAccountResponse(Guid Id, Guid OrganizationId, Guid StudentId, string Currency, string Status, decimal TotalInvoiced, decimal TotalPaid, decimal CreditBalance, decimal OutstandingBalance, string? RestrictionReason, string? SuspensionReason, string? ClosureReason, DateTimeOffset CreatedAtUtc, DateTimeOffset? LastModifiedAtUtc);
public interface IBillingAccountReadService
{
    Task<BillingAccountResponse?> GetByIdAsync(OrganizationId organizationId, BillingAccountId id, CancellationToken cancellationToken = default);
    Task<BillingAccountResponse?> GetByStudentAsync(OrganizationId organizationId, PersonId studentId, CancellationToken cancellationToken = default);
}
public sealed record GetBillingAccountQuery(OrganizationId OrganizationId, BillingAccountId BillingAccountId) : IQuery<BillingAccountResponse>;
public sealed record GetStudentBillingAccountQuery(OrganizationId OrganizationId, PersonId StudentId) : IQuery<BillingAccountResponse>;
