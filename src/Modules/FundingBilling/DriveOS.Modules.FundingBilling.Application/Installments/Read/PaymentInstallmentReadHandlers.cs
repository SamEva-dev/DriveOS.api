using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.FundingBilling.Domain.Installments;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Application.Installments.Read;

internal sealed class GetPaymentInstallmentQueryHandler(IPaymentInstallmentReadService readService)
    : IQueryHandler<GetPaymentInstallmentQuery, PaymentInstallmentResponse>
{
    public async Task<Result<PaymentInstallmentResponse>> Handle(GetPaymentInstallmentQuery query, CancellationToken cancellationToken)
    {
        PaymentInstallmentResponse? item = await readService.GetByIdAsync(query.OrganizationId, query.PaymentInstallmentId, cancellationToken);
        return item is null ? Result.Failure<PaymentInstallmentResponse>(PaymentInstallmentErrors.NotFound) : Result.Success(item);
    }
}

internal sealed class GetBillingAccountInstallmentsQueryHandler(IPaymentInstallmentReadService readService)
    : IQueryHandler<GetBillingAccountInstallmentsQuery, IReadOnlyCollection<PaymentInstallmentResponse>>
{
    public async Task<Result<IReadOnlyCollection<PaymentInstallmentResponse>>> Handle(GetBillingAccountInstallmentsQuery query, CancellationToken cancellationToken)
        => Result.Success(await readService.ListByBillingAccountAsync(query.OrganizationId, query.BillingAccountId, cancellationToken));
}
