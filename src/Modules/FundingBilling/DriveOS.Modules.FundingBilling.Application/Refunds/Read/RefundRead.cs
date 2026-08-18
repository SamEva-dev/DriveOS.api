using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.FundingBilling.Domain.Refunds;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Application.Refunds.Read;

public sealed record RefundResponse(Guid Id, Guid PaymentId, Guid BillingAccountId, decimal Amount, string Currency, string Reason, string Status, string? ProviderReference, string? RejectionReason, string? FailureReason, DateTimeOffset RequestedAtUtc, DateTimeOffset? ApprovedAtUtc, DateTimeOffset? CompletedAtUtc);
public interface IRefundReadService { Task<RefundResponse?> GetAsync(OrganizationId organizationId, RefundId refundId, CancellationToken cancellationToken=default); Task<IReadOnlyCollection<RefundResponse>> ListByPaymentAsync(OrganizationId organizationId, PaymentId paymentId, CancellationToken cancellationToken=default); }
public sealed record GetRefundQuery(OrganizationId OrganizationId, RefundId RefundId) : IQuery<RefundResponse>;
public sealed record GetPaymentRefundsQuery(OrganizationId OrganizationId, PaymentId PaymentId) : IQuery<IReadOnlyCollection<RefundResponse>>;
internal sealed class GetRefundQueryHandler(IRefundReadService read) : IQueryHandler<GetRefundQuery,RefundResponse> { public async Task<Result<RefundResponse>> Handle(GetRefundQuery q,CancellationToken ct){var x=await read.GetAsync(q.OrganizationId,q.RefundId,ct);return x is null?Result.Failure<RefundResponse>(RefundErrors.NotFound):Result.Success(x);} }
internal sealed class GetPaymentRefundsQueryHandler(IRefundReadService read) : IQueryHandler<GetPaymentRefundsQuery,IReadOnlyCollection<RefundResponse>> { public async Task<Result<IReadOnlyCollection<RefundResponse>>> Handle(GetPaymentRefundsQuery q,CancellationToken ct)=>Result.Success(await read.ListByPaymentAsync(q.OrganizationId,q.PaymentId,ct)); }
