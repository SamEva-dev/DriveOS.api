using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.FundingBilling.Domain.Invoices;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Application.Invoices.Read;

internal sealed class GetInvoiceQueryHandler(IInvoiceReadService readService) : IQueryHandler<GetInvoiceQuery, InvoiceResponse>
{
    public async Task<Result<InvoiceResponse>> Handle(GetInvoiceQuery query, CancellationToken cancellationToken)
    {
        InvoiceResponse? response = await readService.GetByIdAsync(query.OrganizationId, query.InvoiceId, cancellationToken);
        return response is null ? Result.Failure<InvoiceResponse>(InvoiceErrors.NotFound) : Result.Success(response);
    }
}

internal sealed class GetBillingAccountInvoicesQueryHandler(IInvoiceReadService readService) : IQueryHandler<GetBillingAccountInvoicesQuery, IReadOnlyCollection<InvoiceResponse>>
{
    public async Task<Result<IReadOnlyCollection<InvoiceResponse>>> Handle(GetBillingAccountInvoicesQuery query, CancellationToken cancellationToken) =>
        Result.Success(await readService.ListByBillingAccountAsync(query.OrganizationId, query.BillingAccountId, cancellationToken));
}
