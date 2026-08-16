using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Application.Transfers;

public sealed class GetInternalTransfersQueryHandler(IInternalTransferService service)
    : IQueryHandler<GetInternalTransfersQuery, IReadOnlyList<InternalTransferResponse>>
{
    public async Task<Result<IReadOnlyList<InternalTransferResponse>>> Handle(
        GetInternalTransfersQuery query,
        CancellationToken ct
    ) => Result.Success(await service.GetAsync(query, ct));
}

public sealed class AnalyzeInternalTransferCommandHandler(IInternalTransferService service)
    : ICommandHandler<AnalyzeInternalTransferCommand, InternalTransferResponse>
{
    public Task<Result<InternalTransferResponse>> Handle(
        AnalyzeInternalTransferCommand command,
        CancellationToken ct
    ) => service.AnalyzeAsync(command, ct);
}

public sealed class ValidateInternalTransferCommandHandler(IInternalTransferService service)
    : ICommandHandler<ValidateInternalTransferCommand, InternalTransferResponse>
{
    public Task<Result<InternalTransferResponse>> Handle(
        ValidateInternalTransferCommand command,
        CancellationToken ct
    ) => service.ValidateAsync(command, ct);
}
