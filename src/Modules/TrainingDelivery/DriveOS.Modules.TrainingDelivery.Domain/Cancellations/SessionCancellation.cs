using DriveOS.Modules.TrainingDelivery.Domain.Cancellations.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using System.Security.Cryptography;
using System.Text;

namespace DriveOS.Modules.TrainingDelivery.Domain.Cancellations;

/// <summary>
/// Aggregate root recording the definitive termination of a training service after execution has actually started.
/// It does not replace Scheduling cancellation: cancellations before the real start remain owned by BC-09.
/// This aggregate freezes the delivered-time facts and the explicit financial/credit/provider decisions that downstream bounded contexts must reconcile.
/// </summary>
public sealed class SessionCancellation : AggregateRoot<SessionCancellationId>, IAuditableEntity
{
    private SessionCancellation() { }
    private SessionCancellation(SessionCancellationId id) : base(id) { }

    /// <summary>Tenant owning the cancellation and defining the mandatory isolation boundary.</summary>
    public OrganizationId OrganizationId { get; private set; }
    /// <summary>Executed Training Delivery session that was definitively stopped.</summary>
    public TrainingSessionId TrainingSessionId { get; private set; }
    /// <summary>Source Scheduling booking retained for correlation and audit only.</summary>
    public BookingId SourceBookingId { get; private set; }
    /// <summary>Organization owning the student file.</summary>
    public OrganizationId StudentOwnerOrganizationId { get; private set; }
    /// <summary>Organization that was effectively delivering the service.</summary>
    public OrganizationId PerformingOrganizationId { get; private set; }
    /// <summary>Student concerned by the terminated execution.</summary>
    public PersonId StudentId { get; private set; }
    /// <summary>Instructor effectively responsible for the running session.</summary>
    public UserId InstructorId { get; private set; }
    /// <summary>Vehicle actually used when one was involved.</summary>
    public Guid? VehicleId { get; private set; }
    /// <summary>Branch/context effectively used by the session.</summary>
    public BranchId? BranchId { get; private set; }
    /// <summary>Actual UTC start of execution.</summary>
    public DateTimeOffset ActualStartAtUtc { get; private set; }
    /// <summary>Actual UTC instant at which the running service definitively stopped.</summary>
    public DateTimeOffset CancelledAtUtc { get; private set; }
    /// <summary>Gross elapsed minutes between actual start and cancellation.</summary>
    public int GrossDurationMinutes { get; private set; }
    /// <summary>Total non-delivered interruption minutes before definitive termination.</summary>
    public int InterruptionDurationMinutes { get; private set; }
    /// <summary>Teaching/service minutes actually delivered before termination.</summary>
    public int DeliveredDurationMinutes { get; private set; }
    /// <summary>Distance observed during the partial session when odometer evidence allows it.</summary>
    public decimal? DistanceKilometers { get; private set; }
    /// <summary>Operational reason explaining why the already-started service was stopped.</summary>
    public SessionCancellationReason Reason { get; private set; }
    /// <summary>Optional factual detail complementing the structured reason.</summary>
    public string? ReasonDetails { get; private set; }
    /// <summary>Explicit downstream billing decision; BC-10 records the decision but does not own invoicing.</summary>
    public SessionCancellationBillingDecision BillingDecision { get; private set; }
    /// <summary>Explicit training-credit decision; BC-07 remains authoritative for the resulting credit movement.</summary>
    public SessionCancellationCreditDecision CreditDecision { get; private set; }
    /// <summary>Credit quantity to consume for a partial-consumption decision.</summary>
    public decimal? PartialCreditQuantity { get; private set; }
    /// <summary>Explicit provider/freelance compensation decision; Marketplace/partner finance remains authoritative for payment.</summary>
    public SessionCancellationProviderCompensationDecision ProviderCompensationDecision { get; private set; }
    /// <summary>Optional explanation supporting exceptional/manual financial decisions.</summary>
    public string? DecisionReason { get; private set; }
    /// <summary>Credit account snapshot inherited from the confirmed booking.</summary>
    public TrainingCreditAccountId? TrainingCreditAccountId { get; private set; }
    /// <summary>Quantity originally reserved for the booking.</summary>
    public decimal? ReservedCreditQuantity { get; private set; }
    /// <summary>Original Funding reservation reference used to release or consume the correct reservation exactly once.</summary>
    public string? CreditReservationReference { get; private set; }
    /// <summary>Pricing reference snapshot used by Billing without making Training Delivery the pricing authority.</summary>
    public string? PricingReference { get; private set; }
    /// <summary>Idempotency key of the definitive cancellation operation.</summary>
    public Guid OperationId { get; private set; }
    /// <summary>Deterministic fingerprint used to reject conflicting retries under the same operation identifier.</summary>
    public string RequestFingerprint { get; private set; } = string.Empty;
    /// <summary>Authenticated user who definitively stopped the session.</summary>
    public UserId CancelledByUserId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<SessionCancellation> Create(
        SessionCancellationId id, OrganizationId organizationId, TrainingSessionId sessionId, BookingId sourceBookingId,
        OrganizationId studentOwnerOrganizationId, OrganizationId performingOrganizationId, PersonId studentId,
        UserId instructorId, Guid? vehicleId, BranchId? branchId, DateTimeOffset actualStartAtUtc, DateTimeOffset cancelledAtUtc,
        int grossDurationMinutes, int interruptionDurationMinutes, int deliveredDurationMinutes, decimal? distanceKilometers,
        SessionCancellationReason reason, string? reasonDetails, SessionCancellationBillingDecision billingDecision,
        SessionCancellationCreditDecision creditDecision, decimal? partialCreditQuantity,
        SessionCancellationProviderCompensationDecision providerCompensationDecision, string? decisionReason,
        TrainingCreditAccountId? trainingCreditAccountId, decimal? reservedCreditQuantity, string? creditReservationReference, string? pricingReference,
        Guid operationId, UserId actor, DateTimeOffset now)
    {
        if (id.IsEmpty || organizationId.IsEmpty || sessionId.IsEmpty || sourceBookingId.IsEmpty || studentOwnerOrganizationId.IsEmpty ||
            performingOrganizationId.IsEmpty || studentId.IsEmpty || instructorId.IsEmpty || operationId == Guid.Empty || actor.IsEmpty ||
            !Enum.IsDefined(reason) || !Enum.IsDefined(billingDecision) || !Enum.IsDefined(creditDecision) || !Enum.IsDefined(providerCompensationDecision))
            return Result.Failure<SessionCancellation>(SessionCancellationErrors.Invalid);
        if (reasonDetails?.Trim().Length > 3000) return Result.Failure<SessionCancellation>(SessionCancellationErrors.ReasonDetailsTooLong);
        if (decisionReason?.Trim().Length > 2000) return Result.Failure<SessionCancellation>(SessionCancellationErrors.DecisionReasonTooLong);
        if (cancelledAtUtc <= actualStartAtUtc || grossDurationMinutes <= 0 || interruptionDurationMinutes < 0 || deliveredDurationMinutes < 0 || deliveredDurationMinutes > grossDurationMinutes)
            return Result.Failure<SessionCancellation>(SessionCancellationErrors.CancelledAtInvalid);
        if (billingDecision == 0) return Result.Failure<SessionCancellation>(SessionCancellationErrors.BillingDecisionInvalid);

        bool hasCredit = trainingCreditAccountId.HasValue && reservedCreditQuantity is > 0 && !string.IsNullOrWhiteSpace(creditReservationReference);
        if (!hasCredit && creditDecision != SessionCancellationCreditDecision.NotApplicable)
            return Result.Failure<SessionCancellation>(SessionCancellationErrors.CreditDecisionInvalid);
        if (hasCredit && creditDecision == SessionCancellationCreditDecision.NotApplicable)
            return Result.Failure<SessionCancellation>(SessionCancellationErrors.CreditDecisionInvalid);
        if (creditDecision == SessionCancellationCreditDecision.ConsumePartial && (!partialCreditQuantity.HasValue || partialCreditQuantity <= 0 || partialCreditQuantity > reservedCreditQuantity))
            return Result.Failure<SessionCancellation>(SessionCancellationErrors.CreditDecisionInvalid);
        if (creditDecision != SessionCancellationCreditDecision.ConsumePartial && partialCreditQuantity.HasValue)
            return Result.Failure<SessionCancellation>(SessionCancellationErrors.CreditDecisionInvalid);

        bool externalProvider = performingOrganizationId != studentOwnerOrganizationId;
        if (!externalProvider && providerCompensationDecision != SessionCancellationProviderCompensationDecision.NotApplicable)
            return Result.Failure<SessionCancellation>(SessionCancellationErrors.ProviderDecisionInvalid);
        if (externalProvider && providerCompensationDecision == SessionCancellationProviderCompensationDecision.NotApplicable)
            return Result.Failure<SessionCancellation>(SessionCancellationErrors.ProviderDecisionInvalid);

        string fingerprint = CalculateRequestFingerprint(cancelledAtUtc, reason, reasonDetails, billingDecision, creditDecision, partialCreditQuantity, providerCompensationDecision, decisionReason);
        DateTimeOffset utcNow = now.ToUniversalTime();
        var cancellation = new SessionCancellation(id)
        {
            OrganizationId = organizationId, TrainingSessionId = sessionId, SourceBookingId = sourceBookingId,
            StudentOwnerOrganizationId = studentOwnerOrganizationId, PerformingOrganizationId = performingOrganizationId, StudentId = studentId,
            InstructorId = instructorId, VehicleId = vehicleId, BranchId = branchId, ActualStartAtUtc = actualStartAtUtc.ToUniversalTime(),
            CancelledAtUtc = cancelledAtUtc.ToUniversalTime(), GrossDurationMinutes = grossDurationMinutes, InterruptionDurationMinutes = interruptionDurationMinutes,
            DeliveredDurationMinutes = deliveredDurationMinutes, DistanceKilometers = distanceKilometers, Reason = reason,
            ReasonDetails = Normalize(reasonDetails), BillingDecision = billingDecision, CreditDecision = creditDecision, PartialCreditQuantity = partialCreditQuantity,
            ProviderCompensationDecision = providerCompensationDecision, DecisionReason = Normalize(decisionReason), TrainingCreditAccountId = trainingCreditAccountId,
            ReservedCreditQuantity = reservedCreditQuantity, CreditReservationReference = Normalize(creditReservationReference), PricingReference = Normalize(pricingReference),
            OperationId = operationId, RequestFingerprint = fingerprint, CancelledByUserId = actor, CreatedAtUtc = utcNow, CreatedByUserId = actor
        };
        cancellation.RaiseDomainEvent(new TrainingSessionCancelledDuringExecutionDomainEvent(sessionId, id, organizationId, studentId, reason, cancellation.CancelledAtUtc, deliveredDurationMinutes, actor));
        return Result.Success(cancellation);
    }

    public bool Matches(Guid operationId, DateTimeOffset cancelledAtUtc, SessionCancellationReason reason, string? reasonDetails,
        SessionCancellationBillingDecision billingDecision, SessionCancellationCreditDecision creditDecision, decimal? partialCreditQuantity,
        SessionCancellationProviderCompensationDecision providerCompensationDecision, string? decisionReason) =>
        OperationId == operationId && RequestFingerprint == CalculateRequestFingerprint(cancelledAtUtc, reason, reasonDetails, billingDecision, creditDecision, partialCreditQuantity, providerCompensationDecision, decisionReason);

    public static string CalculateRequestFingerprint(DateTimeOffset cancelledAtUtc, SessionCancellationReason reason, string? reasonDetails,
        SessionCancellationBillingDecision billingDecision, SessionCancellationCreditDecision creditDecision, decimal? partialCreditQuantity,
        SessionCancellationProviderCompensationDecision providerCompensationDecision, string? decisionReason)
    {
        string input = string.Join("|", cancelledAtUtc.ToUniversalTime().ToString("O"), (int)reason, Normalize(reasonDetails), (int)billingDecision,
            (int)creditDecision, partialCreditQuantity?.ToString(System.Globalization.CultureInfo.InvariantCulture), (int)providerCompensationDecision, Normalize(decisionReason));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input)));
    }

    public void SetCreatedAudit(DateTimeOffset createdAtUtc, UserId? createdByUserId) { CreatedAtUtc = createdAtUtc.ToUniversalTime(); CreatedByUserId = createdByUserId; }
    public void SetModifiedAudit(DateTimeOffset modifiedAtUtc, UserId? modifiedByUserId) { LastModifiedAtUtc = modifiedAtUtc.ToUniversalTime(); LastModifiedByUserId = modifiedByUserId; }
    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
