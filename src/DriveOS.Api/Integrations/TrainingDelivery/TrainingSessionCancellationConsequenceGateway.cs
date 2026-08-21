using DomainRelay.Abstractions;
using DriveOS.Modules.FundingBilling.Application.TrainingCredits.Manage;
using DriveOS.Modules.TrainingDelivery.Application.CancellationConsequences;
using DriveOS.Modules.TrainingDelivery.Application.Consequences;
using DriveOS.Modules.TrainingDelivery.Domain.Cancellations;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Integrations.TrainingDelivery;

internal sealed class TrainingSessionCancellationConsequenceGateway(IMediator mediator) : ITrainingSessionCancellationConsequenceGateway
{
    public async Task<TrainingSessionConsequenceDispatchResult> DispatchAsync(TrainingSessionCancellationConsequenceEnvelope consequence, CancellationToken cancellationToken = default)
    {
        SessionCancellationSnapshot s = consequence.Snapshot;
        if (consequence.Kind == TrainingSessionCancellationConsequenceKind.TrainingCreditReconciliation)
        {
            if (!s.TrainingCreditAccountId.HasValue || s.ReservedCreditQuantity is not > 0 || string.IsNullOrWhiteSpace(s.CreditReservationReference))
                return TrainingSessionConsequenceDispatchResult.PermanentFailure("training-delivery.cancellation.credit-reference-incomplete");

            Result<TrainingCreditMovementId> result;
            string prefix = $"training-session:{s.SessionId.Value:N}:cancellation:{s.CancellationId.Value:N}";
            switch (s.CreditDecision)
            {
                case SessionCancellationCreditDecision.ReleaseAll:
                    result = await mediator.Send(new ReleaseReservedTrainingCreditCommand(s.OrganizationId, s.TrainingCreditAccountId.Value,
                        s.ReservedCreditQuantity.Value, s.CreditReservationReference!, $"{prefix}:credit-release", s.CancelledByUserId), cancellationToken);
                    break;
                case SessionCancellationCreditDecision.ConsumeAll:
                    result = await mediator.Send(new ConsumeReservedTrainingCreditCommand(s.OrganizationId, s.TrainingCreditAccountId.Value,
                        s.ReservedCreditQuantity.Value, s.CreditReservationReference!, $"{prefix}:credit-consumption", s.CancelledByUserId), cancellationToken);
                    break;
                case SessionCancellationCreditDecision.ConsumePartial when s.PartialCreditQuantity is > 0:
                    result = await mediator.Send(new ConsumeReservedTrainingCreditCommand(s.OrganizationId, s.TrainingCreditAccountId.Value,
                        s.PartialCreditQuantity.Value, s.CreditReservationReference!, $"{prefix}:credit-consumption-partial", s.CancelledByUserId), cancellationToken);
                    if (result.IsFailure) break;
                    decimal remainder = decimal.Round(s.ReservedCreditQuantity.Value - s.PartialCreditQuantity.Value, 2, MidpointRounding.AwayFromZero);
                    if (remainder > 0)
                        result = await mediator.Send(new ReleaseReservedTrainingCreditCommand(s.OrganizationId, s.TrainingCreditAccountId.Value,
                            remainder, s.CreditReservationReference!, $"{prefix}:credit-release-remainder", s.CancelledByUserId), cancellationToken);
                    break;
                case SessionCancellationCreditDecision.ManualReview:
                    return TrainingSessionConsequenceDispatchResult.Deferred("funding.cancellation-credit.manual-review-required");
                default:
                    return TrainingSessionConsequenceDispatchResult.PermanentFailure("training-delivery.cancellation.credit-decision-invalid");
            }
            return result.IsSuccess ? TrainingSessionConsequenceDispatchResult.Processed() : TrainingSessionConsequenceDispatchResult.Retry(result.Error.Code, result.Error.MessageKey);
        }

        return consequence.Kind switch
        {
            TrainingSessionCancellationConsequenceKind.BillingReconciliation => TrainingSessionConsequenceDispatchResult.Deferred("billing.cancellation-reconciliation.module-not-implemented"),
            TrainingSessionCancellationConsequenceKind.ProviderCompensation => TrainingSessionConsequenceDispatchResult.Deferred("professional.cancellation-compensation.module-not-implemented"),
            TrainingSessionCancellationConsequenceKind.VehicleUsage => TrainingSessionConsequenceDispatchResult.Deferred("fleet.cancelled-session-usage.module-not-implemented"),
            TrainingSessionCancellationConsequenceKind.AnalyticsMetrics => TrainingSessionConsequenceDispatchResult.Deferred("analytics.cancelled-session.module-not-implemented"),
            TrainingSessionCancellationConsequenceKind.Communication => TrainingSessionConsequenceDispatchResult.Deferred("communication.cancelled-session.module-not-implemented"),
            _ => TrainingSessionConsequenceDispatchResult.PermanentFailure("training-delivery.cancellation.consequence-kind-unsupported")
        };
    }
}
