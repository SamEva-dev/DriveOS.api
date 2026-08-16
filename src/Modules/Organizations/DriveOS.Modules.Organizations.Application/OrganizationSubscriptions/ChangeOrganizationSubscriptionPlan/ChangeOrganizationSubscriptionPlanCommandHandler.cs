using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Domain.Subscriptions;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationSubscriptions.ChangeOrganizationSubscriptionPlan;

public sealed class ChangeOrganizationSubscriptionPlanCommandHandler(
    IOrganizationSubscriptionRepository repository,
    IUnitOfWork unitOfWork
) : ICommandHandler<ChangeOrganizationSubscriptionPlanCommand>
{
    public async Task<Result> Handle(
        ChangeOrganizationSubscriptionPlanCommand command,
        CancellationToken cancellationToken
    )
    {
        var subscription = await repository.GetForUpdateAsync(
            command.OrganizationId,
            cancellationToken
        );
        if (subscription is null)
            return Result.Failure(OrganizationSubscriptionErrors.NotFound);
        if (subscription.Version != command.ExpectedVersion)
            return Result.Failure(OrganizationSubscriptionErrors.ConcurrentUpdate);
        var plan = SubscriptionPlanCode.Create(command.PlanCode);
        if (plan.IsFailure)
            return Result.Failure(plan.Error);
        var result = subscription.ChangePlan(
            plan.Value,
            command.EntitlementCodes,
            command.Limits,
            command.Reason,
            command.ChangedByUserId
        );
        if (result.IsFailure)
            return result;
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}
