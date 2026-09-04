using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Application.Abstractions;
using DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.Cache;
using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.Modules.Organizations.Domain.Subscriptions;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationSubscriptions.CreateOrganizationSubscription;

public sealed class CreateOrganizationSubscriptionCommandHandler(
    IOrganizationReadService organizationReadService,
    IOrganizationSubscriptionRepository repository,
    IOrganizationActivationReadinessCacheInvalidator readinessCacheInvalidator,
    IUnitOfWork unitOfWork
) : ICommandHandler<CreateOrganizationSubscriptionCommand, OrganizationSubscriptionId>
{
    public async Task<Result<OrganizationSubscriptionId>> Handle(
        CreateOrganizationSubscriptionCommand command,
        CancellationToken cancellationToken
    )
    {
        if (
            await organizationReadService.GetByIdAsync(command.OrganizationId, cancellationToken)
            is null
        )
            return Result.Failure<OrganizationSubscriptionId>(OrganizationErrors.NotFound);
        if (await repository.ExistsAsync(command.OrganizationId, cancellationToken))
            return Result.Failure<OrganizationSubscriptionId>(
                OrganizationSubscriptionErrors.AlreadyExists
            );
        if (
            !string.IsNullOrWhiteSpace(command.ExternalProvider)
            && !string.IsNullOrWhiteSpace(command.ExternalSubscriptionId)
            && await repository.ExternalReferenceExistsAsync(
                command.ExternalProvider,
                command.ExternalSubscriptionId,
                null,
                cancellationToken
            )
        )
            return Result.Failure<OrganizationSubscriptionId>(
                OrganizationSubscriptionErrors.ExternalReferenceAlreadyUsed
            );

        Result<SubscriptionPlanCode> planResult = SubscriptionPlanCode.Create(command.PlanCode);
        if (planResult.IsFailure)
            return Result.Failure<OrganizationSubscriptionId>(planResult.Error);
        Result<SubscriptionPeriod> periodResult = SubscriptionPeriod.Create(
            command.CurrentPeriodStartsAtUtc,
            command.CurrentPeriodEndsAtUtc
        );
        if (periodResult.IsFailure)
            return Result.Failure<OrganizationSubscriptionId>(periodResult.Error);

        SubscriptionPeriod? trial = null;
        if (command.TrialStartsAtUtc.HasValue)
        {
            Result<SubscriptionPeriod> trialResult = SubscriptionPeriod.Create(
                command.TrialStartsAtUtc.Value,
                command.TrialEndsAtUtc
            );
            if (trialResult.IsFailure)
                return Result.Failure<OrganizationSubscriptionId>(trialResult.Error);
            trial = trialResult.Value;
        }

        Result<OrganizationSubscription> result = OrganizationSubscription.Create(
            OrganizationSubscriptionId.New(),
            command.OrganizationId,
            planResult.Value,
            command.Status,
            command.BillingCycle,
            periodResult.Value,
            trial,
            command.ExternalProvider,
            command.ExternalSubscriptionId
        );
        if (result.IsFailure)
            return Result.Failure<OrganizationSubscriptionId>(result.Error);

        await repository.AddAsync(result.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        readinessCacheInvalidator.Invalidate(command.OrganizationId);
        return Result.Success(result.Value.Id);
    }
}
