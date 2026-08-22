using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.ExamsCertification.Application.Persistence;
using DriveOS.Modules.ExamsCertification.Application.Places.Sync;
using DriveOS.Modules.ExamsCertification.Application.Providers;
using DriveOS.Modules.ExamsCertification.Domain.Places.Watch;
using DriveOS.Modules.ExamsCertification.Domain.Providers;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Application.Places.Watch;

public sealed class CreateExamPlaceWatchCommandHandler(
    IExamPlaceProviderResolver providerResolver,
    IExamPlaceWatchRepository repository,
    IExamsCertificationUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<CreateExamPlaceWatchCommand, ExamPlaceWatchSubscriptionId>
{
    public async Task<Result<ExamPlaceWatchSubscriptionId>> Handle(CreateExamPlaceWatchCommand command, CancellationToken cancellationToken)
    {
        IExamPlaceProvider? provider = providerResolver.Resolve(command.ProviderCode);
        if (provider is null || !provider.Descriptor.IsEnabled)
            return Result.Failure<ExamPlaceWatchSubscriptionId>(ExamPlaceSynchronizationErrors.ProviderNotFound);
        if (!provider.Descriptor.Capabilities.HasFlag(ExamPlaceProviderCapability.ReadAvailablePlaces))
            return Result.Failure<ExamPlaceWatchSubscriptionId>(ExamPlaceSynchronizationErrors.ProviderDoesNotExposeAvailability);

        ExamPlaceWatchSubscriptionId id = ExamPlaceWatchSubscriptionId.New();
        Result<ExamPlaceWatchSubscription> creation = ExamPlaceWatchSubscription.Create(
            id, command.OrganizationId, provider.Descriptor.Code, command.CountryCode, command.AdministrativeAreaCode,
            command.ExamCategory, command.WindowFromUtc, command.WindowToUtc, command.CheckIntervalMinutes,
            command.CenterExternalIds, clock.UtcNow);
        if (creation.IsFailure) return Result.Failure<ExamPlaceWatchSubscriptionId>(creation.Error);

        creation.Value.SetCreatedAudit(clock.UtcNow, command.ActorUserId);
        repository.Add(creation.Value);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(id);
    }
}

public sealed class GetExamPlaceWatchesQueryHandler(IExamPlaceWatchRepository repository)
    : IQueryHandler<GetExamPlaceWatchesQuery, IReadOnlyList<ExamPlaceWatchResponse>>
{
    public async Task<Result<IReadOnlyList<ExamPlaceWatchResponse>>> Handle(GetExamPlaceWatchesQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyList<ExamPlaceWatchSubscription> items = await repository.ListAsync(query.OrganizationId, cancellationToken);
        return Result.Success<IReadOnlyList<ExamPlaceWatchResponse>>(items.Select(Map).ToArray());
    }

    private static ExamPlaceWatchResponse Map(ExamPlaceWatchSubscription x) => new(
        x.Id.Value, x.ProviderCode, x.CountryCode, x.AdministrativeAreaCode, x.ExamCategory, x.WindowFromUtc,
        x.WindowToUtc, x.CheckIntervalMinutes, x.GetCenterExternalIds(), x.Status.ToString(), x.NextCheckAtUtc,
        x.LastCheckedAtUtc, x.LastSuccessfulCheckAtUtc, x.LastAvailabilityDetectedAtUtc, x.LastErrorCode,
        x.ConsecutiveFailureCount);
}

public sealed class GetExamPlaceWatchScansQueryHandler(IExamPlaceWatchRepository repository)
    : IQueryHandler<GetExamPlaceWatchScansQuery, IReadOnlyList<ExamPlaceWatchScanResponse>>
{
    public async Task<Result<IReadOnlyList<ExamPlaceWatchScanResponse>>> Handle(GetExamPlaceWatchScansQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyList<ExamPlaceWatchScan> items = await repository.ListScansAsync(query.OrganizationId, query.SubscriptionId, query.Take, cancellationToken);
        return Result.Success<IReadOnlyList<ExamPlaceWatchScanResponse>>(items.Select(x => new ExamPlaceWatchScanResponse(
            x.Id.Value, x.StartedAtUtc, x.CompletedAtUtc, x.IsSuccess, x.ExternalSlotsRead, x.NewAvailabilitiesDetected, x.ErrorCode)).ToArray());
    }
}

public sealed class PauseExamPlaceWatchCommandHandler(IExamPlaceWatchRepository repository, IExamsCertificationUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<PauseExamPlaceWatchCommand>
{
    public async Task<Result> Handle(PauseExamPlaceWatchCommand command, CancellationToken cancellationToken)
    {
        ExamPlaceWatchSubscription? subscription = await repository.GetByIdForUpdateAsync(command.OrganizationId, command.SubscriptionId, cancellationToken);
        if (subscription is null) return Result.Failure(ExamPlaceWatchErrors.NotFound);
        Result result = subscription.Pause(command.ActorUserId, clock.UtcNow);
        if (result.IsFailure) return result;
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class ResumeExamPlaceWatchCommandHandler(IExamPlaceWatchRepository repository, IExamsCertificationUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<ResumeExamPlaceWatchCommand>
{
    public async Task<Result> Handle(ResumeExamPlaceWatchCommand command, CancellationToken cancellationToken)
    {
        ExamPlaceWatchSubscription? subscription = await repository.GetByIdForUpdateAsync(command.OrganizationId, command.SubscriptionId, cancellationToken);
        if (subscription is null) return Result.Failure(ExamPlaceWatchErrors.NotFound);
        Result result = subscription.Resume(command.ActorUserId, clock.UtcNow);
        if (result.IsFailure) return result;
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class RunExamPlaceWatchCommandHandler(
    IMediator mediator,
    IExamPlaceWatchRepository repository,
    IExamsCertificationUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<RunExamPlaceWatchCommand, ExamPlaceWatchRunResponse>
{
    public async Task<Result<ExamPlaceWatchRunResponse>> Handle(RunExamPlaceWatchCommand command, CancellationToken cancellationToken)
    {
        ExamPlaceWatchSubscription? subscription = await repository.GetByIdForUpdateAsync(command.OrganizationId, command.SubscriptionId, cancellationToken);
        if (subscription is null) return Result.Failure<ExamPlaceWatchRunResponse>(ExamPlaceWatchErrors.NotFound);
        if (subscription.Status != ExamPlaceWatchStatus.Active)
            return Result.Failure<ExamPlaceWatchRunResponse>(ExamPlaceWatchErrors.NotActive);

        DateTimeOffset startedAtUtc = clock.UtcNow;
        ExamPlaceWatchScan scan = ExamPlaceWatchScan.Start(ExamPlaceWatchScanId.New(), subscription.Id, subscription.OrganizationId, startedAtUtc);
        repository.Add(scan);

        Result<ExamPlaceSynchronizationResponse> synchronization = await mediator.Send(new SynchronizeExamPlacesCommand(
            subscription.OrganizationId,
            subscription.ProviderCode,
            subscription.CountryCode,
            subscription.AdministrativeAreaCode,
            subscription.ExamCategory,
            subscription.WindowFromUtc,
            subscription.WindowToUtc,
            subscription.GetCenterExternalIds(),
            command.ActorUserId), cancellationToken);

        DateTimeOffset completedAtUtc = clock.UtcNow;
        if (synchronization.IsFailure)
        {
            subscription.RecordFailedScan(completedAtUtc, synchronization.Error.Code, command.ActorUserId);
            scan.Fail(completedAtUtc, synchronization.Error.Code);
            await unitOfWork.CommitAsync(cancellationToken);
            return Result.Success(new ExamPlaceWatchRunResponse(subscription.Id.Value, false, completedAtUtc, 0, 0, synchronization.Error.Code));
        }

        int newlyDetected = 0;
        foreach (Guid rawPlaceId in synchronization.Value.ObservedAvailablePlaceIds.Distinct())
        {
            ExamPlaceId placeId = new(rawPlaceId);
            if (await repository.HitExistsAsync(subscription.Id, placeId, cancellationToken))
                continue;

            repository.Add(ExamPlaceWatchHit.Create(ExamPlaceWatchHitId.New(), subscription.Id, subscription.OrganizationId, placeId, completedAtUtc));
            subscription.RecordNewAvailability(placeId, completedAtUtc);
            newlyDetected++;
        }

        subscription.RecordSuccessfulScan(completedAtUtc, newlyDetected > 0, command.ActorUserId);
        scan.Complete(completedAtUtc, synchronization.Value.ExternalSlotsRead, newlyDetected);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(new ExamPlaceWatchRunResponse(subscription.Id.Value, true, completedAtUtc,
            synchronization.Value.ExternalSlotsRead, newlyDetected, null));
    }
}
