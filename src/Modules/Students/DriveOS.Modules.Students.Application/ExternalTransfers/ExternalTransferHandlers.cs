using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Application.ExternalTransfers;

public sealed class GetExternalTransfersQueryHandler(IExternalTransferService s)
    : IQueryHandler<GetExternalTransfersQuery, IReadOnlyList<ExternalTransferResponse>>
{
    public async Task<Result<IReadOnlyList<ExternalTransferResponse>>> Handle(
        GetExternalTransfersQuery q,
        CancellationToken ct
    ) => Result.Success(await s.GetAsync(q, ct));
}

public sealed class CreateExternalTransferCommandHandler(IExternalTransferService s)
    : ICommandHandler<CreateExternalTransferCommand, Guid>
{
    public Task<Result<Guid>> Handle(CreateExternalTransferCommand c, CancellationToken ct) =>
        s.CreateAsync(c, ct);
}

public sealed class VerifyExternalTransferConsentCommandHandler(IExternalTransferService s)
    : ICommandHandler<VerifyExternalTransferConsentCommand>
{
    public Task<Result> Handle(VerifyExternalTransferConsentCommand c, CancellationToken ct) =>
        s.VerifyConsentAsync(c, ct);
}

public sealed class ReviewExternalTransferFinanceCommandHandler(IExternalTransferService s)
    : ICommandHandler<ReviewExternalTransferFinanceCommand>
{
    public Task<Result> Handle(ReviewExternalTransferFinanceCommand c, CancellationToken ct) =>
        s.ReviewFinanceAsync(c, ct);
}

public sealed class SubmitExternalTransferCommandHandler(IExternalTransferService s)
    : ICommandHandler<SubmitExternalTransferCommand, ExternalTransferPreconditions>
{
    public Task<Result<ExternalTransferPreconditions>> Handle(
        SubmitExternalTransferCommand c,
        CancellationToken ct
    ) => s.SubmitAsync(c, ct);
}

public sealed class DecideExternalTransferCommandHandler(IExternalTransferService s)
    : ICommandHandler<DecideExternalTransferCommand>
{
    public Task<Result> Handle(DecideExternalTransferCommand c, CancellationToken ct) =>
        s.DecideAsync(c, ct);
}

public sealed class CompleteExternalTransferCommandHandler(IExternalTransferService s)
    : ICommandHandler<CompleteExternalTransferCommand>
{
    public Task<Result> Handle(CompleteExternalTransferCommand c, CancellationToken ct) =>
        s.CompleteAsync(c, ct);
}
