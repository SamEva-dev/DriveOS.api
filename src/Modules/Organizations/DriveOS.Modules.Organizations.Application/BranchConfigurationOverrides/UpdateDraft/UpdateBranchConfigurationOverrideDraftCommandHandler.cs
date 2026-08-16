using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Domain.BranchConfigurationOverrides;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.BranchConfigurationOverrides.UpdateDraft;

public sealed class UpdateBranchConfigurationOverrideDraftCommandHandler(
    IBranchConfigurationOverrideRepository repository,
    IUnitOfWork unitOfWork
) : ICommandHandler<UpdateBranchConfigurationOverrideDraftCommand>
{
    public async Task<Result> Handle(
        UpdateBranchConfigurationOverrideDraftCommand command,
        CancellationToken cancellationToken
    )
    {
        var branchOverride = await repository.GetForUpdateAsync(
            command.OverrideId,
            command.OrganizationId,
            command.BranchId,
            cancellationToken
        );
        if (branchOverride is null)
            return Result.Failure(BranchConfigurationOverrideErrors.NotFound);
        if (branchOverride.Revision != command.ExpectedRevision)
            return Result.Failure(BranchConfigurationOverrideErrors.ConcurrentUpdate);

        Result<BranchOverridePayload> payload = BranchOverridePayload.Create(command.PayloadJson);
        if (payload.IsFailure)
            return Result.Failure(payload.Error);
        Result result = branchOverride.UpdateDraft(payload.Value);
        if (result.IsFailure)
            return result;
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}
