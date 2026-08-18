using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Application.Auditing;

public sealed class GetFinancialAuditQueryHandler(IFinancialAuditReadService readService)
    : IQueryHandler<GetFinancialAuditQuery, IReadOnlyList<FinancialAuditEntryResponse>>
{
    public async Task<Result<IReadOnlyList<FinancialAuditEntryResponse>>> Handle(
        GetFinancialAuditQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<FinancialAuditEntryResponse> entries = await readService.ListAsync(
            request.OrganizationId,
            request.BillingAccountId,
            cancellationToken);

        return Result.Success(entries);
    }
}
