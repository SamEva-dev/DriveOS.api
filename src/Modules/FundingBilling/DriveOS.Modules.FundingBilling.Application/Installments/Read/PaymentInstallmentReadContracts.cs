using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Application.Installments.Read;

public sealed record PaymentInstallmentResponse(
    Guid Id,
    Guid BillingAccountId,
    DateOnly DueDate,
    decimal ExpectedAmount,
    decimal PaidAmount,
    decimal RemainingAmount,
    string Currency,
    Guid? FinancingPersonId,
    Guid? FinancingOrganizationId,
    string Status,
    DateOnly? PreviousDueDate,
    string? LastReason,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? LastModifiedAtUtc);

public interface IPaymentInstallmentReadService
{
    Task<PaymentInstallmentResponse?> GetByIdAsync(OrganizationId organizationId, PaymentInstallmentId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PaymentInstallmentResponse>> ListByBillingAccountAsync(OrganizationId organizationId, BillingAccountId billingAccountId, CancellationToken cancellationToken = default);
}

public sealed record GetPaymentInstallmentQuery(OrganizationId OrganizationId, PaymentInstallmentId PaymentInstallmentId) : IQuery<PaymentInstallmentResponse>;
public sealed record GetBillingAccountInstallmentsQuery(OrganizationId OrganizationId, BillingAccountId BillingAccountId) : IQuery<IReadOnlyCollection<PaymentInstallmentResponse>>;
