using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Domain.TrainingCredits.Events;

public sealed record TrainingCreditMovementRecordedDomainEvent(
    TrainingCreditAccountId AccountId,
    TrainingCreditMovementId MovementId,
    TrainingCreditMovementType Type,
    decimal Quantity,
    string Reference,
    UserId ActorUserId,
    DateTimeOffset CreditMovementRecordedAtUtc) : DomainEvent;
