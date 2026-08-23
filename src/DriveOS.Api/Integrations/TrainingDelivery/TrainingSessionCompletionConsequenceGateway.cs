using DomainRelay.Abstractions;
using DriveOS.Modules.FundingBilling.Application.TrainingCredits.Manage;
using DriveOS.Application.Abstractions.Integrations.RegulatoryTrainingRecords;
using DriveOS.Modules.RegulatoryIntegrations.Application.Submissions;
using DriveOS.Modules.TrainingDelivery.Application.Consequences;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Api.Integrations.TrainingDelivery;

internal sealed class TrainingSessionCompletionConsequenceGateway(
    IMediator mediator,
    IRegulatoryTrainingSessionProjector regulatoryProjector,
    IRegulatoryTrainingRecordSubmissionService regulatorySubmissions) : ITrainingSessionCompletionConsequenceGateway
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
            case TrainingSessionConsequenceKind.RegulatoryTrainingRecordSubmission:
            {
                var projectionResult = await regulatoryProjector.ProjectAsync(new RegulatoryTrainingSessionProjectionSource(
                    snapshot.OrganizationId,
                    snapshot.StudentOwnerOrganizationId,
                    snapshot.PerformingOrganizationId,
                    snapshot.SessionId,
                    snapshot.StudentId,
                    snapshot.TrainingPathId,
                    snapshot.InstructorId,
                    snapshot.BranchId,
                    snapshot.VehicleId,
                    snapshot.TrainingCategory,
                    snapshot.ActualStartAtUtc,
                    snapshot.ActualEndAtUtc,
                    snapshot.DeliveredDurationMinutes,
                    snapshot.CompletedAtUtc), cancellationToken);

                if (projectionResult.IsFailure)
                    return TrainingSessionConsequenceDispatchResult.Retry(projectionResult.Error.Code, projectionResult.Error.MessageKey);

                await regulatorySubmissions.EnsureAsync(projectionResult.Value, cancellationToken);
                return TrainingSessionConsequenceDispatchResult.Processed();
            }
            default:
                return TrainingSessionConsequenceDispatchResult.PermanentFailure("training-delivery.consequence-kind-unsupported");
        }
    }
}
