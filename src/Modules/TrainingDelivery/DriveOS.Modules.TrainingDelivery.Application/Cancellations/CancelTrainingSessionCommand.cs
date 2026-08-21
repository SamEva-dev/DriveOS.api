using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.TrainingDelivery.Domain.Cancellations;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Application.Cancellations;

public sealed record CancelTrainingSessionCommand(
    OrganizationId OrganizationId, TrainingSessionId SessionId, Guid OperationId, DateTimeOffset CancelledAtUtc,
    SessionCancellationReason Reason, string? ReasonDetails, SessionCancellationBillingDecision BillingDecision,
    SessionCancellationCreditDecision CreditDecision, decimal? PartialCreditQuantity,
    SessionCancellationProviderCompensationDecision ProviderCompensationDecision, string? DecisionReason, UserId ActorUserId)
    : ICommand<SessionCancellationResponse>;
