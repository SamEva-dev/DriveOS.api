using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.CurriculumPedagogy.Application.Persistence;
using DriveOS.Modules.CurriculumPedagogy.Domain.PedagogicalReviews;
using DriveOS.Modules.CurriculumPedagogy.Domain.TrainingPaths;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CurriculumPedagogy.Application.PedagogicalReviews;

public sealed class RequestPedagogicalReviewCommandHandler(ITrainingPathRepository trainingPaths, IPedagogicalReviewRepository reviews, ICurriculumPedagogyUnitOfWork unitOfWork, IClock clock) : ICommandHandler<RequestPedagogicalReviewCommand, PedagogicalReviewId>
{
    public async Task<Result<PedagogicalReviewId>> Handle(RequestPedagogicalReviewCommand command, CancellationToken cancellationToken)
    {
        TrainingPath? path = await trainingPaths.GetByIdAsync(command.TrainingPathId, command.OrganizationId, cancellationToken);
        if (path is null) return Result.Failure<PedagogicalReviewId>(PedagogicalReviewApplicationErrors.TrainingPathNotFound);
        if (path.Status is TrainingPathStatus.Draft or TrainingPathStatus.ReadyForActivation or TrainingPathStatus.Cancelled)
            return Result.Failure<PedagogicalReviewId>(PedagogicalReviewApplicationErrors.TrainingPathNotEligible);
        if (await reviews.HasOpenReviewAsync(command.OrganizationId, command.TrainingPathId, cancellationToken))
            return Result.Failure<PedagogicalReviewId>(PedagogicalReviewApplicationErrors.OpenReviewAlreadyExists);
        Result<PedagogicalReview> created = PedagogicalReview.Request(PedagogicalReviewId.New(), command.OrganizationId, path.StudentId, path.Id, command.ReviewerId, command.Reason, clock.UtcNow);
        if (created.IsFailure) return Result.Failure<PedagogicalReviewId>(created.Error);
        await reviews.AddAsync(created.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(created.Value.Id);
    }
}

public abstract class ReviewMutationHandlerBase(IPedagogicalReviewRepository reviews, ICurriculumPedagogyUnitOfWork unitOfWork)
{
    protected async Task<Result<PedagogicalReview>> LoadAsync(OrganizationId organizationId, PedagogicalReviewId reviewId, CancellationToken ct)
    {
        PedagogicalReview? review = await reviews.GetByIdForUpdateAsync(organizationId, reviewId, ct);
        return review is null ? Result.Failure<PedagogicalReview>(PedagogicalReviewApplicationErrors.ReviewNotFound) : Result.Success(review);
    }
    protected async Task<Result> CommitAsync(Result result, CancellationToken ct) { if (result.IsFailure) return result; await unitOfWork.CommitAsync(ct); return Result.Success(); }
}

public sealed class StartPedagogicalReviewCommandHandler(IPedagogicalReviewRepository reviews, ICurriculumPedagogyUnitOfWork unitOfWork, IClock clock) : ReviewMutationHandlerBase(reviews, unitOfWork), ICommandHandler<StartPedagogicalReviewCommand>
{
    public async Task<Result> Handle(StartPedagogicalReviewCommand command, CancellationToken ct) { var loaded = await LoadAsync(command.OrganizationId, command.ReviewId, ct); return loaded.IsFailure ? Result.Failure(loaded.Error) : await CommitAsync(loaded.Value.Start(clock.UtcNow), ct); }
}
public sealed class CompletePedagogicalReviewCommandHandler(IPedagogicalReviewRepository reviews, ICurriculumPedagogyUnitOfWork unitOfWork, IClock clock) : ReviewMutationHandlerBase(reviews, unitOfWork), ICommandHandler<CompletePedagogicalReviewCommand>
{
    public async Task<Result> Handle(CompletePedagogicalReviewCommand command, CancellationToken ct) { var loaded = await LoadAsync(command.OrganizationId, command.ReviewId, ct); return loaded.IsFailure ? Result.Failure(loaded.Error) : await CommitAsync(loaded.Value.Complete(command.Findings, command.Recommendations, command.EstimatedRemainingPracticalHours, clock.UtcNow), ct); }
}
public sealed class CancelPedagogicalReviewCommandHandler(IPedagogicalReviewRepository reviews, ICurriculumPedagogyUnitOfWork unitOfWork, IClock clock) : ReviewMutationHandlerBase(reviews, unitOfWork), ICommandHandler<CancelPedagogicalReviewCommand>
{
    public async Task<Result> Handle(CancelPedagogicalReviewCommand command, CancellationToken ct) { var loaded = await LoadAsync(command.OrganizationId, command.ReviewId, ct); return loaded.IsFailure ? Result.Failure(loaded.Error) : await CommitAsync(loaded.Value.Cancel(command.Reason, clock.UtcNow), ct); }
}
