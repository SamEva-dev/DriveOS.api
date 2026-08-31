using DriveOS.Modules.ProfessionalMarketplace.Domain.CommercialOffers;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ProfessionalMarketplace.Domain.Engagements;

/// <summary>
/// Operational professional relationship created from a finalized commercial offer.
/// The accepted commercial terms are snapshotted and immutable for the lifetime of this engagement.
/// Subsequent operational missions, access grants and scheduling assignments reference this aggregate.
/// </summary>
public sealed class ProfessionalEngagement : AggregateRoot<ProfessionalEngagementId>, IAuditableEntity
{
    private ProfessionalEngagement() { }

    private ProfessionalEngagement(
        ProfessionalEngagementId id,
        OrganizationId organizationId,
        BranchId? branchId,
        ProfessionalProfileId professionalProfileId,
        ProfessionalCommercialOfferId commercialOfferId,
        int commercialOfferRevision,
        CommercialOfferTerms termsSnapshot) : base(id)
    {
        OrganizationId = organizationId;
        BranchId = branchId;
        ProfessionalProfileId = professionalProfileId;
        CommercialOfferId = commercialOfferId;
        CommercialOfferRevision = commercialOfferRevision;
        TermsSnapshot = termsSnapshot;
        StartsOn = termsSnapshot.StartsOn;
        EndsOn = termsSnapshot.EndsOn;
        Status = ProfessionalEngagementStatus.PendingActivation;
    }

    public OrganizationId OrganizationId { get; private set; }
    public BranchId? BranchId { get; private set; }
    public ProfessionalProfileId ProfessionalProfileId { get; private set; }
    public ProfessionalCommercialOfferId CommercialOfferId { get; private set; }
    public int CommercialOfferRevision { get; private set; }
    public CommercialOfferTerms TermsSnapshot { get; private set; } = default!;
    public DateOnly StartsOn { get; private set; }
    public DateOnly EndsOn { get; private set; }
    public ProfessionalEngagementStatus Status { get; private set; }
    public bool CompliancePrepared { get; private set; }
    public bool ContractPrepared { get; private set; }
    public bool AccessPrepared { get; private set; }
    public bool SchedulingPrepared { get; private set; }
    public bool InternalApprovalPrepared { get; private set; }
    public DateTimeOffset? ActivatedAtUtc { get; private set; }
    public DateTimeOffset? SuspendedAtUtc { get; private set; }
    public DateTimeOffset? EndedAtUtc { get; private set; }
    public DateTimeOffset? InitialIntegrationCompletedAtUtc { get; private set; }
    public ProfessionalInvoiceId? FirstPaidProfessionalInvoiceId { get; private set; }
    public Guid? FirstPaidFinanceSupplierInvoiceId { get; private set; }
    public Guid? FirstPaymentAttemptId { get; private set; }
    public string? ConfirmedPaymentMethod { get; private set; }
    public DateTimeOffset? ReliableRelationshipEstablishedAtUtc { get; private set; }
    public DateTimeOffset? SatisfactionRequestedAtUtc { get; private set; }
    public bool IsReliableRelationship => ReliableRelationshipEstablishedAtUtc is not null;
    public string? StatusReason { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public bool IsOperationallyReady =>
        CompliancePrepared &&
        ContractPrepared &&
        AccessPrepared &&
        SchedulingPrepared &&
        InternalApprovalPrepared;

    public static Result<ProfessionalEngagement> Create(
        ProfessionalEngagementId id,
        BranchId? branchId,
        ProfessionalCommercialOffer offer,
        DateTimeOffset now,
        UserId actor)
    {
        if (id.IsEmpty || offer.Id.IsEmpty)
            return Result.Failure<ProfessionalEngagement>(ProfessionalEngagementErrors.InvalidIdentifier);

        if (offer.Status != ProfessionalCommercialOfferStatus.Finalized || offer.FinalizedAtUtc is null)
            return Result.Failure<ProfessionalEngagement>(ProfessionalEngagementErrors.FinalizedOfferRequired);

        var engagement = new ProfessionalEngagement(
            id,
            offer.OrganizationId,
            branchId,
            offer.ProfessionalProfileId,
            offer.Id,
            offer.Revision,
            offer.Terms);

        engagement.SetCreatedAudit(now, actor);
        return Result.Success(engagement);
    }

    public Result MarkPreparation(
        EngagementPreparationStep step,
        bool completed,
        DateTimeOffset now,
        UserId actor)
    {
        if (Status is ProfessionalEngagementStatus.Ended or ProfessionalEngagementStatus.Terminated)
            return Result.Failure(ProfessionalEngagementErrors.InvalidTransition);

        switch (step)
        {
            case EngagementPreparationStep.Compliance:
                CompliancePrepared = completed;
                break;
            case EngagementPreparationStep.Contract:
                ContractPrepared = completed;
                break;
            case EngagementPreparationStep.Access:
                AccessPrepared = completed;
                break;
            case EngagementPreparationStep.Scheduling:
                SchedulingPrepared = completed;
                break;
            case EngagementPreparationStep.InternalApproval:
                InternalApprovalPrepared = completed;
                break;
            default:
                return Result.Failure(ProfessionalEngagementErrors.InvalidPreparationStep);
        }

        SetModifiedAudit(now, actor);
        return Result.Success();
    }

    public Result Activate(DateOnly today, DateTimeOffset now, UserId actor)
    {
        if (Status is not ProfessionalEngagementStatus.PendingActivation and not ProfessionalEngagementStatus.Suspended)
            return Result.Failure(ProfessionalEngagementErrors.InvalidTransition);

        if (!IsOperationallyReady)
            return Result.Failure(ProfessionalEngagementErrors.PreparationIncomplete);

        if (today < StartsOn || today > EndsOn)
            return Result.Failure(ProfessionalEngagementErrors.OutsideEngagementPeriod);

        Status = ProfessionalEngagementStatus.Active;
        ActivatedAtUtc ??= now.ToUniversalTime();
        SuspendedAtUtc = null;
        StatusReason = null;
        SetModifiedAudit(now, actor);
        return Result.Success();
    }

    public Result Suspend(string reason, DateTimeOffset now, UserId actor)
    {
        if (Status != ProfessionalEngagementStatus.Active)
            return Result.Failure(ProfessionalEngagementErrors.InvalidTransition);

        reason = NormalizeReason(reason);
        if (reason.Length < 2)
            return Result.Failure(ProfessionalEngagementErrors.StatusReasonRequired);

        Status = ProfessionalEngagementStatus.Suspended;
        SuspendedAtUtc = now.ToUniversalTime();
        StatusReason = reason;
        SetModifiedAudit(now, actor);
        return Result.Success();
    }

    public Result Resume(DateOnly today, DateTimeOffset now, UserId actor)
    {
        if (Status != ProfessionalEngagementStatus.Suspended)
            return Result.Failure(ProfessionalEngagementErrors.InvalidTransition);

        if (!IsOperationallyReady)
            return Result.Failure(ProfessionalEngagementErrors.PreparationIncomplete);

        if (today < StartsOn || today > EndsOn)
            return Result.Failure(ProfessionalEngagementErrors.OutsideEngagementPeriod);

        Status = ProfessionalEngagementStatus.Active;
        SuspendedAtUtc = null;
        StatusReason = null;
        SetModifiedAudit(now, actor);
        return Result.Success();
    }

    public Result CompleteInitialIntegration(
        ProfessionalInvoiceId professionalInvoiceId,
        Guid financeSupplierInvoiceId,
        Guid? paymentAttemptId,
        string? paymentMethod,
        DateTimeOffset nowUtc)
    {
        if(professionalInvoiceId.IsEmpty||financeSupplierInvoiceId==Guid.Empty)
            return Result.Failure(ProfessionalEngagementErrors.InvalidIdentifier);

        if(InitialIntegrationCompletedAtUtc is not null)
            return Result.Success();

        InitialIntegrationCompletedAtUtc=nowUtc.ToUniversalTime();
        FirstPaidProfessionalInvoiceId=professionalInvoiceId;
        FirstPaidFinanceSupplierInvoiceId=financeSupplierInvoiceId;
        FirstPaymentAttemptId=paymentAttemptId;
        ConfirmedPaymentMethod=NormalizePaymentMethod(paymentMethod);
        ReliableRelationshipEstablishedAtUtc=nowUtc.ToUniversalTime();
        SetModifiedAudit(nowUtc,null);
        return Result.Success();
    }

    public Result MarkSatisfactionRequested(DateTimeOffset nowUtc)
    {
        if(InitialIntegrationCompletedAtUtc is null)
            return Result.Failure(ProfessionalEngagementErrors.InvalidTransition);

        SatisfactionRequestedAtUtc??=nowUtc.ToUniversalTime();
        SetModifiedAudit(nowUtc,null);
        return Result.Success();
    }

    public Result Complete(DateOnly today, DateTimeOffset now, UserId actor)
    {
        if (Status is not ProfessionalEngagementStatus.Active and not ProfessionalEngagementStatus.Suspended)
            return Result.Failure(ProfessionalEngagementErrors.InvalidTransition);

        if (today < EndsOn)
            return Result.Failure(ProfessionalEngagementErrors.EngagementNotEndedYet);

        Status = ProfessionalEngagementStatus.Ended;
        EndedAtUtc = now.ToUniversalTime();
        SetModifiedAudit(now, actor);
        return Result.Success();
    }

    public Result Terminate(string reason, DateTimeOffset now, UserId actor)
    {
        if (Status is ProfessionalEngagementStatus.Ended or ProfessionalEngagementStatus.Terminated)
            return Result.Failure(ProfessionalEngagementErrors.InvalidTransition);

        reason = NormalizeReason(reason);
        if (reason.Length < 2)
            return Result.Failure(ProfessionalEngagementErrors.StatusReasonRequired);

        Status = ProfessionalEngagementStatus.Terminated;
        StatusReason = reason;
        EndedAtUtc = now.ToUniversalTime();
        SetModifiedAudit(now, actor);
        return Result.Success();
    }

    public void SetCreatedAudit(DateTimeOffset at, UserId? actor)
    {
        CreatedAtUtc = at.ToUniversalTime();
        CreatedByUserId = actor;
    }

    public void SetModifiedAudit(DateTimeOffset at, UserId? actor)
    {
        LastModifiedAtUtc = at.ToUniversalTime();
        LastModifiedByUserId = actor;
    }

    private static string NormalizeReason(string? value)
    {
        string result = (value ?? string.Empty).Trim();
        return result[..Math.Min(result.Length, 512)];
    }

    private static string? NormalizePaymentMethod(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        string result = value.Trim().ToUpperInvariant();
        return result[..Math.Min(result.Length, 80)];
    }
}

public enum ProfessionalEngagementStatus
{
    PendingActivation = 1,
    Active = 2,
    Suspended = 3,
    Ended = 4,
    Terminated = 5
}

public enum EngagementPreparationStep
{
    Compliance = 1,
    Contract = 2,
    Access = 3,
    Scheduling = 4,
    InternalApproval = 5
}
