using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.FundingBilling.Domain.Payments;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Application.Payments.Read;

public sealed record PaymentAllocationResponse(Guid Id, Guid? InvoiceId, Guid? InstallmentId, decimal Amount, DateTimeOffset AllocatedAtUtc, Guid AllocatedByUserId);
public sealed record PaymentResponse(Guid Id, Guid BillingAccountId, Guid? PayerPersonId, Guid? PayerOrganizationId,
    decimal Amount, decimal AllocatedAmount, decimal UnallocatedAmount, decimal RefundedAmount, decimal RefundableAmount, string Currency, string PaymentMethod,
    string? ExternalReference, string Status, DateTimeOffset? PaidAtUtc, IReadOnlyCollection<PaymentAllocationResponse> Allocations);

public interface IPaymentReadService
{
    Task<PaymentResponse?> GetAsync(OrganizationId organizationId, PaymentId paymentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PaymentResponse>> ListByBillingAccountAsync(OrganizationId organizationId, BillingAccountId billingAccountId, CancellationToken cancellationToken = default);
}

public sealed record GetPaymentQuery(OrganizationId OrganizationId, PaymentId PaymentId) : IQuery<PaymentResponse>;
public sealed record GetBillingAccountPaymentsQuery(OrganizationId OrganizationId, BillingAccountId BillingAccountId) : IQuery<IReadOnlyCollection<PaymentResponse>>;

internal sealed class GetPaymentQueryHandler(IPaymentReadService read) : IQueryHandler<GetPaymentQuery, PaymentResponse>
{
    public async Task<Result<PaymentResponse>> Handle(GetPaymentQuery q,CancellationToken ct){var r=await read.GetAsync(q.OrganizationId,q.PaymentId,ct);return r is null?Result.Failure<PaymentResponse>(PaymentErrors.NotFound):Result.Success(r);}
}
internal sealed class GetBillingAccountPaymentsQueryHandler(IPaymentReadService read) : IQueryHandler<GetBillingAccountPaymentsQuery, IReadOnlyCollection<PaymentResponse>>
{
    public async Task<Result<IReadOnlyCollection<PaymentResponse>>> Handle(GetBillingAccountPaymentsQuery q,CancellationToken ct)=>Result.Success(await read.ListByBillingAccountAsync(q.OrganizationId,q.BillingAccountId,ct));
}
