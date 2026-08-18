using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.FundingBilling.Domain.BillingAccounts;
using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.FundingBilling.Application.BillingAccounts.Read;
public sealed class GetBillingAccountQueryHandler(IBillingAccountReadService read) : IQueryHandler<GetBillingAccountQuery, BillingAccountResponse>
{
    public async Task<Result<BillingAccountResponse>> Handle(GetBillingAccountQuery query, CancellationToken ct)
    {
        BillingAccountResponse? value = await read.GetByIdAsync(query.OrganizationId, query.BillingAccountId, ct);
        return value is null ? Result.Failure<BillingAccountResponse>(BillingAccountErrors.NotFound) : Result.Success(value);
    }
}
public sealed class GetStudentBillingAccountQueryHandler(IBillingAccountReadService read) : IQueryHandler<GetStudentBillingAccountQuery, BillingAccountResponse>
{
    public async Task<Result<BillingAccountResponse>> Handle(GetStudentBillingAccountQuery query, CancellationToken ct)
    {
        BillingAccountResponse? value = await read.GetByStudentAsync(query.OrganizationId, query.StudentId, ct);
        return value is null ? Result.Failure<BillingAccountResponse>(BillingAccountErrors.NotFound) : Result.Success(value);
    }
}
