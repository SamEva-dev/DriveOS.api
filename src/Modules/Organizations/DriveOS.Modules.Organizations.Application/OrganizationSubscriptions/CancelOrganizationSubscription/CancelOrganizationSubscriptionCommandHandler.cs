using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Domain.Subscriptions;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationSubscriptions.CancelOrganizationSubscription;

public sealed class CancelOrganizationSubscriptionCommandHandler(
    IOrganizationSubscriptionRepository repository,
    IUnitOfWork unitOfWork
) : ICommandHandler<CancelOrganizationSubscriptionCommand>
{
    public async Task<Result> Handle(
        CancelOrganizationSubscriptionCommand command,
        CancellationToken cancellationToken
    )
    {
        OrganizationSubscription? subscription = await repository.GetForUpdateAsync(
            command.OrganizationId,
            cancellationToken
        );
        if (subscription is null)
            return Result.Failure(OrganizationSubscriptionErrors.NotFound);
        if (subscription.Version != command.ExpectedVersion)
            return Result.Failure(OrganizationSubscriptionErrors.ConcurrentUpdate);
        Result<SubscriptionCancellation> cancellation = SubscriptionCancellation.Create(
            command.RequestedAtUtc,
            command.EffectiveAtUtc,
            command.Reason,
            command.RequestedByUserId
        );
        if (cancellation.IsFailure)
            return Result.Failure(cancellation.Error);
        Result result = subscription.Cancel(cancellation.Value);
        if (result.IsFailure)
            return result;
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}
