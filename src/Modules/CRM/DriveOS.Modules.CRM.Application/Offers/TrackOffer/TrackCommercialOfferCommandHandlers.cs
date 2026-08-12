using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.CRM.Application.Abstractions.Persistence;
using DriveOS.Modules.CRM.Domain.Offers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Application.Offers.TrackOffer;

internal sealed class RecordCommercialOfferViewCommandHandler(ICommercialOfferRepository offers,
    ICrmUnitOfWork unitOfWork, IClock clock) : ICommandHandler<RecordCommercialOfferViewCommand>
{
    public async Task<Result> Handle(RecordCommercialOfferViewCommand command, CancellationToken cancellationToken)
    {
        CommercialOffer? offer = await offers.GetForUpdateBySecureTokenHashAsync(command.SecureTokenHash, cancellationToken);
        if (offer is null) return Result.Failure(CommercialOfferErrors.NotFound);
        Result result = offer.RecordView(clock.UtcNow);
        if (result.IsFailure) return result;
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class RecordCommercialOfferExchangeCommandHandler(ICommercialOfferRepository offers,
    ICrmUnitOfWork unitOfWork, IClock clock, ICurrentUser currentUser)
    : ICommandHandler<RecordCommercialOfferExchangeCommand>
{
    public async Task<Result> Handle(RecordCommercialOfferExchangeCommand command, CancellationToken cancellationToken)
    {
        CommercialOffer? offer = await offers.GetForUpdateAsync(command.OrganizationId, command.OfferId, cancellationToken);
        if (offer is null) return Result.Failure(CommercialOfferErrors.NotFound);
        Result result = offer.RecordExchange(command.Type, command.Summary, command.MetadataJson,
            currentUser.UserId, clock.UtcNow);
        if (result.IsFailure) return result;
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class ScheduleCommercialOfferFollowUpCommandHandler(ICommercialOfferRepository offers,
    ICrmUnitOfWork unitOfWork, IClock clock, ICurrentUser currentUser)
    : ICommandHandler<ScheduleCommercialOfferFollowUpCommand>
{
    public async Task<Result> Handle(ScheduleCommercialOfferFollowUpCommand command, CancellationToken cancellationToken)
    {
        CommercialOffer? offer = await offers.GetForUpdateAsync(command.OrganizationId, command.OfferId, cancellationToken);
        if (offer is null) return Result.Failure(CommercialOfferErrors.NotFound);
        Result result = offer.ScheduleFollowUp(command.NextFollowUpAtUtc, command.Note, currentUser.UserId, clock.UtcNow);
        if (result.IsFailure) return result;
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class WithdrawCommercialOfferCommandHandler(ICommercialOfferRepository offers,
    ICrmUnitOfWork unitOfWork, IClock clock, ICurrentUser currentUser)
    : ICommandHandler<WithdrawCommercialOfferCommand>
{
    public async Task<Result> Handle(WithdrawCommercialOfferCommand command, CancellationToken cancellationToken)
    {
        CommercialOffer? offer = await offers.GetForUpdateAsync(command.OrganizationId, command.OfferId, cancellationToken);
        if (offer is null) return Result.Failure(CommercialOfferErrors.NotFound);
        Result result = offer.Withdraw(command.Reason, currentUser.UserId, clock.UtcNow);
        if (result.IsFailure) return result;
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class MarkCommercialOfferAcceptedCommandHandler(ICommercialOfferRepository offers,
    ICrmUnitOfWork unitOfWork, IClock clock) : ICommandHandler<MarkCommercialOfferAcceptedCommand>
{
    public async Task<Result> Handle(MarkCommercialOfferAcceptedCommand command, CancellationToken cancellationToken)
    {
        CommercialOffer? offer = await offers.GetForUpdateAsync(command.OrganizationId, command.OfferId, cancellationToken);
        if (offer is null) return Result.Failure(CommercialOfferErrors.NotFound);
        Result result = offer.Accept(clock.UtcNow);
        if (result.IsFailure) return result;
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class MarkCommercialOfferRejectedCommandHandler(ICommercialOfferRepository offers,
    ICrmUnitOfWork unitOfWork, IClock clock, ICurrentUser currentUser)
    : ICommandHandler<MarkCommercialOfferRejectedCommand>
{
    public async Task<Result> Handle(MarkCommercialOfferRejectedCommand command, CancellationToken cancellationToken)
    {
        CommercialOffer? offer = await offers.GetForUpdateAsync(command.OrganizationId, command.OfferId, cancellationToken);
        if (offer is null) return Result.Failure(CommercialOfferErrors.NotFound);
        Result exchange = offer.RecordExchange(OfferInteractionType.QuestionReceived,
            command.Reason, "{\"decision\":\"rejected\"}", currentUser.UserId, clock.UtcNow);
        if (exchange.IsFailure) return exchange;
        Result result = offer.Reject(clock.UtcNow);
        if (result.IsFailure) return result;
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}
