using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Domain.Subscriptions;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationSubscriptions.ChangeStatus;

public sealed class ChangeOrganizationSubscriptionStatusCommandHandler(
    IOrganizationSubscriptionRepository repository,
    IUnitOfWork unitOfWork
) : ICommandHandler<ChangeOrganizationSubscriptionStatusCommand>
{
    public async Task<Result> Handle(
        ChangeOrganizationSubscriptionStatusCommand command,
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

        Result result;
        if (command.TargetStatus == SubscriptionStatus.Active)
        {
            if (!command.PeriodStartsAtUtc.HasValue)
                return Result.Failure(OrganizationSubscriptionErrors.InvalidPeriod);
            Result<SubscriptionPeriod> period = SubscriptionPeriod.Create(
                command.PeriodStartsAtUtc.Value,
                command.PeriodEndsAtUtc
            );
            if (period.IsFailure)
                return Result.Failure(period.Error);
            result = subscription.Activate(period.Value, command.Reason, command.ChangedByUserId);
        }
        else
        {
            result = command.TargetStatus switch
            {
                SubscriptionStatus.PastDue => subscription.MarkPastDue(
                    command.Reason,
                    command.ChangedByUserId
                ),
                SubscriptionStatus.Restricted => subscription.Restrict(
                    command.Reason,
                    command.ChangedByUserId
                ),
                SubscriptionStatus.Suspended => subscription.Suspend(
                    command.Reason,
                    command.ChangedByUserId
                ),
                SubscriptionStatus.Expired => subscription.Expire(
                    command.Reason,
                    command.ChangedByUserId
                ),
                _ => Result.Failure(OrganizationSubscriptionErrors.InvalidStatusTransition),
            };
        }

        if (result.IsFailure)
            return result;
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}
