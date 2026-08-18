using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Contracts.Application.Auditing;

public sealed class GetContractAuditQueryHandler(IContractAuditReadService readService)
    : IQueryHandler<GetContractAuditQuery, IReadOnlyList<ContractAuditEntryResponse>>
{
    public async Task<Result<IReadOnlyList<ContractAuditEntryResponse>>> Handle(
        GetContractAuditQuery query,
        CancellationToken cancellationToken) =>
        Result.Success(await readService.ListAsync(
            query.OrganizationId,
            query.ContractId,
            cancellationToken));
}
