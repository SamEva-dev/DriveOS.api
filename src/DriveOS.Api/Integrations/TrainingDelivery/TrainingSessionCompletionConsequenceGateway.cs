using DomainRelay.Abstractions;
using DriveOS.Modules.FundingBilling.Application.TrainingCredits.Manage;
using DriveOS.Modules.TrainingDelivery.Application.Consequences;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Api.Integrations.TrainingDelivery;

internal sealed class TrainingSessionCompletionConsequenceGateway(IMediator mediator) : ITrainingSessionCompletionConsequenceGateway
{
    public async Task<TrainingSessionConsequenceDispatchResult> DispatchAsync(TrainingSessionConsequenceEnvelope consequence, CancellationToken cancellationToken = default)
    {
        TrainingSessionCompletionSnapshot snapshot = consequence.Snapshot;
        switch (consequence.Kind)
        {
            case TrainingSessionConsequenceKind.TrainingCreditConsumption:
                if (!snapshot.TrainingCreditAccountId.HasValue || snapshot.CreditQuantity is not > 0 || string.IsNullOrWhiteSpace(snapshot.CreditReservationReference))
                    return TrainingSessionConsequenceDispatchResult.PermanentFailure("training-delivery.credit-reference-incomplete");
                var result = await mediator.Send(new ConsumeReservedTrainingCreditCommand(
                    snapshot.OrganizationId,
                    snapshot.TrainingCreditAccountId.Value,
                    snapshot.CreditQuantity.Value,
                    snapshot.CreditReservationReference!,
                    $"training-session:{snapshot.SessionId.Value:N}:credit-consumption",
                    snapshot.CompletedByUserId), cancellationToken);
                if (result.IsSuccess)
                    return TrainingSessionConsequenceDispatchResult.Processed();

                bool permanent = result.Error.Code is
                    "FundingBilling.TrainingCreditAccount.NotFound" or
                    "FundingBilling.TrainingCreditAccount.Reserved.Insufficient" or
                    "FundingBilling.TrainingCreditAccount.Movement.ReferenceDuplicate" or
                    "FundingBilling.TrainingCreditAccount.Operation.NotAllowed";
                return permanent
                    ? TrainingSessionConsequenceDispatchResult.PermanentFailure(result.Error.Code, result.Error.MessageKey)
                    : TrainingSessionConsequenceDispatchResult.Retry(result.Error.Code, result.Error.MessageKey);

            case TrainingSessionConsequenceKind.BillingServiceRecognition:
                return TrainingSessionConsequenceDispatchResult.Deferred("billing.service-recognition.module-not-implemented");
            case TrainingSessionConsequenceKind.ProfessionalServiceEntry:
                return TrainingSessionConsequenceDispatchResult.Deferred("professional.service-entry.module-not-implemented");
            case TrainingSessionConsequenceKind.VehicleUsage:
                return TrainingSessionConsequenceDispatchResult.Deferred("fleet.vehicle-usage.module-not-implemented");
            case TrainingSessionConsequenceKind.AnalyticsMetrics:
                return TrainingSessionConsequenceDispatchResult.Deferred("analytics.session-metrics.module-not-implemented");
            case TrainingSessionConsequenceKind.SessionSummaryCommunication:
                return TrainingSessionConsequenceDispatchResult.Deferred("communication.session-summary.module-not-implemented");
            default:
                return TrainingSessionConsequenceDispatchResult.PermanentFailure("training-delivery.consequence-kind-unsupported");
        }
    }
}
