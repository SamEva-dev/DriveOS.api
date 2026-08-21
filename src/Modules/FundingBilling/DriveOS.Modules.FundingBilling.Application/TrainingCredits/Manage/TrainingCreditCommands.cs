using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.FundingBilling.Application.Persistence;
using DriveOS.Modules.FundingBilling.Domain.TrainingCredits;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using FluentValidation;

namespace DriveOS.Modules.FundingBilling.Application.TrainingCredits.Manage;

public enum TrainingCreditOperation { Purchase = 1, Reserve = 2, Release = 3, Consume = 4, Adjust = 5 }

public sealed record RecordTrainingCreditMovementCommand(OrganizationId OrganizationId, TrainingCreditAccountId AccountId,
    TrainingCreditOperation Operation, decimal Quantity, string Reference, string? Reason, UserId ActorUserId) : ICommand<TrainingCreditMovementId>;

internal sealed class RecordTrainingCreditMovementCommandValidator : AbstractValidator<RecordTrainingCreditMovementCommand>
{
    public RecordTrainingCreditMovementCommandValidator()
    {
        RuleFor(x => x.OrganizationId.Value).NotEmpty();
        RuleFor(x => x.AccountId.Value).NotEmpty();
        RuleFor(x => x.Quantity).NotEqual(0m);
        RuleFor(x => x.Reference).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Reason).MaximumLength(1000);
        RuleFor(x => x.ActorUserId.Value).NotEmpty();
        RuleFor(x => x).Must(x => x.Operation == TrainingCreditOperation.Adjust || x.Quantity > 0m).WithMessage("Quantity must be positive for this operation.");
        RuleFor(x => x.Reason).NotEmpty().When(x => x.Operation == TrainingCreditOperation.Adjust);
    }
}

internal sealed class RecordTrainingCreditMovementCommandHandler(ITrainingCreditAccountRepository accounts,
    IFundingBillingUnitOfWork unitOfWork, IClock clock) : ICommandHandler<RecordTrainingCreditMovementCommand, TrainingCreditMovementId>
{
    public async Task<Result<TrainingCreditMovementId>> Handle(RecordTrainingCreditMovementCommand command, CancellationToken cancellationToken)
    {
        TrainingCreditAccount? account = await accounts.GetByIdAsync(command.AccountId, cancellationToken);
        if (account is null || account.OrganizationId != command.OrganizationId)
            return Result.Failure<TrainingCreditMovementId>(TrainingCreditAccountErrors.NotFound);
        string normalizedReference = command.Reference.Trim();
        TrainingCreditMovement? existingMovement = await accounts.GetMovementByReferenceAsync(account.Id, normalizedReference, cancellationToken);
        if (existingMovement is not null)
        {
            TrainingCreditMovementType expectedType = command.Operation switch
            {
                TrainingCreditOperation.Purchase => TrainingCreditMovementType.Purchase,
                TrainingCreditOperation.Reserve => TrainingCreditMovementType.Reservation,
                TrainingCreditOperation.Release => TrainingCreditMovementType.Release,
                TrainingCreditOperation.Consume => TrainingCreditMovementType.Consumption,
                TrainingCreditOperation.Adjust => TrainingCreditMovementType.Adjustment,
                _ => 0
            };

            decimal expectedQuantity = decimal.Round(command.Quantity, 2, MidpointRounding.AwayFromZero);
            return existingMovement.Type == expectedType && existingMovement.Quantity == expectedQuantity
                ? Result.Success(existingMovement.Id)
                : Result.Failure<TrainingCreditMovementId>(TrainingCreditAccountErrors.MovementReferenceDuplicate);
        }

        DateTimeOffset now = clock.UtcNow;
        TrainingCreditMovementId movementId = TrainingCreditMovementId.New();
        Result<TrainingCreditMovementId> result = command.Operation switch
        {
            TrainingCreditOperation.Purchase => account.Purchase(movementId, command.Quantity, command.Reference, command.Reason, command.ActorUserId, now),
            TrainingCreditOperation.Reserve => account.Reserve(movementId, command.Quantity, command.Reference, command.Reason, command.ActorUserId, now),
            TrainingCreditOperation.Release => account.Release(movementId, command.Quantity, command.Reference, command.Reason, command.ActorUserId, now),
            TrainingCreditOperation.Consume => account.Consume(movementId, command.Quantity, command.Reference, command.Reason, command.ActorUserId, now),
            TrainingCreditOperation.Adjust => account.Adjust(movementId, command.Quantity, command.Reference, command.Reason ?? string.Empty, command.ActorUserId, now),
            _ => Result.Failure<TrainingCreditMovementId>(TrainingCreditAccountErrors.MovementInvalid)
        };
        if (result.IsFailure) return result;
        account.SetModifiedAudit(now, command.ActorUserId);
        await unitOfWork.CommitAsync(cancellationToken);
        return result;
    }
}

public sealed record ConsumeReservedTrainingCreditCommand(
    OrganizationId OrganizationId,
    TrainingCreditAccountId AccountId,
    decimal Quantity,
    string ReservationReference,
    string ConsumptionReference,
    UserId ActorUserId) : ICommand<TrainingCreditMovementId>;

internal sealed class ConsumeReservedTrainingCreditCommandHandler(
    ITrainingCreditAccountRepository accounts,
    IFundingBillingUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<ConsumeReservedTrainingCreditCommand, TrainingCreditMovementId>
{
    public async Task<Result<TrainingCreditMovementId>> Handle(ConsumeReservedTrainingCreditCommand command, CancellationToken cancellationToken)
    {
        TrainingCreditAccount? account = await accounts.GetByIdAsync(command.AccountId, cancellationToken);
        if (account is null || account.OrganizationId != command.OrganizationId)
            return Result.Failure<TrainingCreditMovementId>(TrainingCreditAccountErrors.NotFound);

        string reservationReference = command.ReservationReference.Trim();
        string consumptionReference = command.ConsumptionReference.Trim();
        TrainingCreditMovement? existingConsumption = account.Movements.SingleOrDefault(x => x.Reference == consumptionReference);
        if (existingConsumption is not null)
            return existingConsumption.Type == TrainingCreditMovementType.Consumption && existingConsumption.Quantity == decimal.Round(command.Quantity, 2, MidpointRounding.AwayFromZero)
                ? Result.Success(existingConsumption.Id)
                : Result.Failure<TrainingCreditMovementId>(TrainingCreditAccountErrors.MovementReferenceDuplicate);

        TrainingCreditMovement? reservation = account.Movements.SingleOrDefault(x => x.Reference == reservationReference);
        if (reservation is null || reservation.Type != TrainingCreditMovementType.Reservation || reservation.Quantity < decimal.Round(command.Quantity, 2, MidpointRounding.AwayFromZero))
            return Result.Failure<TrainingCreditMovementId>(TrainingCreditAccountErrors.InsufficientReserved);

        DateTimeOffset now = clock.UtcNow;
        TrainingCreditMovementId movementId = TrainingCreditMovementId.New();
        Result<TrainingCreditMovementId> consumed = account.Consume(
            movementId,
            command.Quantity,
            consumptionReference,
            $"Consumption of reservation {reservationReference}",
            command.ActorUserId,
            now);
        if (consumed.IsFailure) return consumed;
        account.SetModifiedAudit(now, command.ActorUserId);
        await unitOfWork.CommitAsync(cancellationToken);
        return consumed;
    }
}


public sealed record ReleaseReservedTrainingCreditCommand(
    OrganizationId OrganizationId,
    TrainingCreditAccountId AccountId,
    decimal Quantity,
    string ReservationReference,
    string ReleaseReference,
    UserId ActorUserId) : ICommand<TrainingCreditMovementId>;

internal sealed class ReleaseReservedTrainingCreditCommandHandler(
    ITrainingCreditAccountRepository accounts,
    IFundingBillingUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<ReleaseReservedTrainingCreditCommand, TrainingCreditMovementId>
{
    public async Task<Result<TrainingCreditMovementId>> Handle(ReleaseReservedTrainingCreditCommand command, CancellationToken cancellationToken)
    {
        TrainingCreditAccount? account = await accounts.GetByIdAsync(command.AccountId, cancellationToken);
        if (account is null || account.OrganizationId != command.OrganizationId)
            return Result.Failure<TrainingCreditMovementId>(TrainingCreditAccountErrors.NotFound);

        string reservationReference = command.ReservationReference.Trim();
        string releaseReference = command.ReleaseReference.Trim();
        TrainingCreditMovement? existingRelease = account.Movements.SingleOrDefault(x => x.Reference == releaseReference);
        if (existingRelease is not null)
            return existingRelease.Type == TrainingCreditMovementType.Release && existingRelease.Quantity == decimal.Round(command.Quantity, 2, MidpointRounding.AwayFromZero)
                ? Result.Success(existingRelease.Id)
                : Result.Failure<TrainingCreditMovementId>(TrainingCreditAccountErrors.MovementReferenceDuplicate);

        TrainingCreditMovement? reservation = account.Movements.SingleOrDefault(x => x.Reference == reservationReference);
        if (reservation is null || reservation.Type != TrainingCreditMovementType.Reservation || reservation.Quantity < decimal.Round(command.Quantity, 2, MidpointRounding.AwayFromZero))
            return Result.Failure<TrainingCreditMovementId>(TrainingCreditAccountErrors.InsufficientReserved);

        DateTimeOffset now = clock.UtcNow;
        TrainingCreditMovementId movementId = TrainingCreditMovementId.New();
        Result<TrainingCreditMovementId> released = account.Release(movementId, command.Quantity, releaseReference,
            $"Release of reservation {reservationReference}", command.ActorUserId, now);
        if (released.IsFailure) return released;
        account.SetModifiedAudit(now, command.ActorUserId);
        await unitOfWork.CommitAsync(cancellationToken);
        return released;
    }
}
