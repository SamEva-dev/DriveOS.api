using DriveOS.Modules.CRM.Domain.Leads.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Domain.Leads;

public sealed class Lead : AggregateRoot<LeadId>, IAuditableEntity
{
    private Lead()
    {
    }

    private Lead(
        LeadId id,
        OrganizationId organizationId,
        BranchId? branchId,
        LeadIdentity identity,
        RequestedTraining requestedTraining,
        LeadSource source,
        UserId? assignedAdvisorId)
        : base(id)
    {
        OrganizationId = organizationId;
        BranchId = branchId;
        Identity = identity;
        RequestedTraining = requestedTraining;
        Source = source;
        AssignedAdvisorId = assignedAdvisorId;
        Status = LeadStatus.New;
        AutomaticFollowUpsEnabled = true;
    }

    public OrganizationId OrganizationId { get; private set; }

    public BranchId? BranchId { get; private set; }

    public LeadIdentity Identity { get; private set; } = null!;

    public RequestedTraining RequestedTraining { get; private set; } = null!;

    public LeadSource Source { get; private set; } = null!;

    public UserId? AssignedAdvisorId { get; private set; }

    public LeadStatus Status { get; private set; }

    public LeadQualification? Qualification { get; private set; }

    public PersonId? ConvertedPersonId { get; private set; }

    public DraftEnrollmentId? DraftEnrollmentId { get; private set; }

    public DateTimeOffset? ConvertedAtUtc { get; private set; }

    public LeadClosureReason? ClosureReason { get; private set; }
    public string? ClosureComment { get; private set; }
    public DateTimeOffset? ClosedAtUtc { get; private set; }
    public DateTimeOffset? ResumeAtUtc { get; private set; }
    public UserId? DormancyResponsibleUserId { get; private set; }
    public string? DormancyCampaignCode { get; private set; }
    public string? ReferredPartnerName { get; private set; }
    public string? SharedDataDescription { get; private set; }
    public DateTimeOffset? ReferralConsentCollectedAtUtc { get; private set; }
    public DateTimeOffset? ReopenedAtUtc { get; private set; }
    public bool AutomaticFollowUpsEnabled { get; private set; } = true;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public UserId? CreatedByUserId { get; private set; }

    public DateTimeOffset? LastModifiedAtUtc { get; private set; }

    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<Lead> Create(
        LeadId id,
        OrganizationId organizationId,
        BranchId? branchId,
        LeadIdentity identity,
        RequestedTraining requestedTraining,
        LeadSource source,
        UserId? assignedAdvisorId = null)
    {
        if (id.IsEmpty)
        {
            return Result.Failure<Lead>(LeadErrors.EmptyId);
        }

        if (organizationId.IsEmpty)
        {
            return Result.Failure<Lead>(LeadErrors.EmptyOrganizationId);
        }

        ArgumentNullException.ThrowIfNull(identity);
        ArgumentNullException.ThrowIfNull(requestedTraining);
        ArgumentNullException.ThrowIfNull(source);

        var lead = new Lead(
            id,
            organizationId,
            branchId,
            identity,
            requestedTraining,
            source,
            assignedAdvisorId);

        lead.RaiseDomainEvent(
            new LeadCreatedDomainEvent(
                lead.Id,
                lead.OrganizationId,
                lead.BranchId,
                lead.Identity.FirstName,
                lead.Identity.LastName,
                lead.RequestedTraining.LicenseCategory,
                lead.Source.Type));

        return Result.Success(lead);
    }

    public Result UpdateInformation(
        BranchId? branchId,
        LeadIdentity identity,
        RequestedTraining requestedTraining,
        LeadSource source)
    {
        ArgumentNullException.ThrowIfNull(identity);
        ArgumentNullException.ThrowIfNull(requestedTraining);
        ArgumentNullException.ThrowIfNull(source);

        BranchId = branchId;
        Identity = identity;
        RequestedTraining = requestedTraining;
        Source = source;

        RaiseDomainEvent(
            new LeadInformationUpdatedDomainEvent(
                Id,
                OrganizationId,
                BranchId));

        return Result.Success();
    }

    public Result ChangeStatus(LeadStatus targetStatus, string? reason = null)
    {
        if (!Enum.IsDefined(targetStatus))
        {
            return Result.Failure(LeadErrors.InvalidStatus);
        }

        if (Status == targetStatus)
        {
            return Result.Failure(LeadErrors.StatusAlreadyApplied);
        }

        if (!CanTransition(Status, targetStatus))
        {
            return Result.Failure(LeadErrors.InvalidStatusTransition);
        }

        string? normalizedReason = string.IsNullOrWhiteSpace(reason)
            ? null
            : reason.Trim();

        if (targetStatus == LeadStatus.Lost && normalizedReason is null)
        {
            return Result.Failure(LeadErrors.LossReasonRequired);
        }

        if (normalizedReason?.Length > 500)
        {
            return Result.Failure(LeadErrors.StatusReasonTooLong);
        }

        LeadStatus previousStatus = Status;
        Status = targetStatus;
        RaiseDomainEvent(new LeadStatusChangedDomainEvent(
            Id,
            OrganizationId,
            previousStatus,
            targetStatus,
            normalizedReason));

        return Result.Success();
    }

    public Result Qualify(LeadQualification qualification)
    {
        ArgumentNullException.ThrowIfNull(qualification);

        if (Status == LeadStatus.Contacted)
        {
            Status = LeadStatus.Qualified;
        }
        else if (Status is not LeadStatus.Qualified and not LeadStatus.AssessmentScheduled)
        {
            return Result.Failure(LeadErrors.QualificationNotAllowed);
        }

        Qualification = qualification;
        RaiseDomainEvent(new LeadQualifiedDomainEvent(Id, OrganizationId));
        return Result.Success();
    }

    public Result Close(LeadStatus decision, LeadClosureReason reason, string? comment,
        DateTimeOffset closedAtUtc)
    {
        if (!IsClosureDecision(decision) || !Enum.IsDefined(reason))
            return Result.Failure(LeadErrors.InvalidClosureDecision);
        if (Status is LeadStatus.Won or LeadStatus.TransferredToPartner)
            return Result.Failure(LeadErrors.InvalidStatusTransition);
        if (comment?.Trim().Length > 1000)
            return Result.Failure(LeadErrors.ClosureCommentTooLong);

        LeadStatus previous = Status;
        Status = decision;
        ClosureReason = reason;
        ClosureComment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        ClosedAtUtc = closedAtUtc.ToUniversalTime();
        AutomaticFollowUpsEnabled = false;
        ClearDormancy();
        RaiseDomainEvent(new LeadStatusChangedDomainEvent(Id, OrganizationId, previous, decision, ClosureComment));
        RaiseDomainEvent(new LeadMarkedLostDomainEvent(Id, OrganizationId, decision, reason, ClosureComment));
        return Result.Success();
    }

    public Result SetDormant(LeadClosureReason reason, DateTimeOffset resumeAtUtc,
        UserId responsibleUserId, string? campaignCode, string? comment, DateTimeOffset nowUtc)
    {
        if (!Enum.IsDefined(reason)) return Result.Failure(LeadErrors.InvalidClosureReason);
        if (responsibleUserId.IsEmpty) return Result.Failure(LeadErrors.DormancyResponsibleRequired);
        if (resumeAtUtc.ToUniversalTime() <= nowUtc.ToUniversalTime())
            return Result.Failure(LeadErrors.ResumeDateMustBeFuture);
        if (campaignCode?.Trim().Length > 100) return Result.Failure(LeadErrors.CampaignCodeTooLong);

        Status = LeadStatus.Dormant;
        ClosureReason = reason;
        ClosureComment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        ClosedAtUtc = nowUtc.ToUniversalTime();
        ResumeAtUtc = resumeAtUtc.ToUniversalTime();
        DormancyResponsibleUserId = responsibleUserId;
        DormancyCampaignCode = string.IsNullOrWhiteSpace(campaignCode) ? null : campaignCode.Trim();
        AutomaticFollowUpsEnabled = false;
        RaiseDomainEvent(new LeadMarkedDormantDomainEvent(Id, OrganizationId, reason,
            ResumeAtUtc.Value, responsibleUserId));
        return Result.Success();
    }

    public Result ReferToPartner(string partnerName, string sharedDataDescription,
        DateTimeOffset consentCollectedAtUtc, string? comment, DateTimeOffset referredAtUtc)
    {
        if (string.IsNullOrWhiteSpace(partnerName) || partnerName.Trim().Length > 200)
            return Result.Failure(LeadErrors.PartnerNameInvalid);
        if (string.IsNullOrWhiteSpace(sharedDataDescription) || sharedDataDescription.Trim().Length > 2000)
            return Result.Failure(LeadErrors.SharedDataDescriptionInvalid);
        if (consentCollectedAtUtc == default || consentCollectedAtUtc > referredAtUtc)
            return Result.Failure(LeadErrors.ReferralConsentRequired);

        Status = LeadStatus.TransferredToPartner;
        ClosureReason = LeadClosureReason.PartnerReferral;
        ClosureComment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        ClosedAtUtc = referredAtUtc.ToUniversalTime();
        ReferredPartnerName = partnerName.Trim();
        SharedDataDescription = sharedDataDescription.Trim();
        ReferralConsentCollectedAtUtc = consentCollectedAtUtc.ToUniversalTime();
        AutomaticFollowUpsEnabled = false;
        RaiseDomainEvent(new LeadReferredToPartnerDomainEvent(Id, OrganizationId,
            ReferredPartnerName, SharedDataDescription, ReferralConsentCollectedAtUtc.Value));
        return Result.Success();
    }

    public Result Reopen(string? comment, DateTimeOffset reopenedAtUtc)
    {
        if (!IsClosed(Status)) return Result.Failure(LeadErrors.ReopenNotAllowed);
        LeadStatus previous = Status;
        Status = LeadStatus.New;
        ReopenedAtUtc = reopenedAtUtc.ToUniversalTime();
        AutomaticFollowUpsEnabled = true;
        ClosureComment = string.IsNullOrWhiteSpace(comment) ? ClosureComment : comment.Trim();
        ClearDormancy();
        RaiseDomainEvent(new LeadReopenedDomainEvent(Id, OrganizationId, previous, comment?.Trim()));
        return Result.Success();
    }

    private void ClearDormancy()
    {
        ResumeAtUtc = null;
        DormancyResponsibleUserId = null;
        DormancyCampaignCode = null;
    }

    private static bool IsClosureDecision(LeadStatus status) => status is LeadStatus.Lost
        or LeadStatus.NotEligible or LeadStatus.OutOfScope or LeadStatus.Duplicate
        or LeadStatus.NoResponse or LeadStatus.CancelledByLead or LeadStatus.ConvertedElsewhere;

    private static bool IsClosed(LeadStatus status) => IsClosureDecision(status)
        || status is LeadStatus.Dormant or LeadStatus.TransferredToPartner;

    public Result MarkConverted(PersonId personId, DraftEnrollmentId draftEnrollmentId, DateTimeOffset convertedAtUtc)
    {
        if (ConvertedPersonId.HasValue && DraftEnrollmentId.HasValue)
        {
            return ConvertedPersonId.Value == personId && DraftEnrollmentId.Value == draftEnrollmentId
                ? Result.Success()
                : Result.Failure(LeadErrors.AlreadyConverted);
        }

        if (Status != LeadStatus.Won)
        {
            return Result.Failure(LeadErrors.ConversionRequiresWonStatus);
        }

        if (Qualification is null)
        {
            return Result.Failure(LeadErrors.ConversionRequiresQualification);
        }

        if (personId.IsEmpty || draftEnrollmentId.IsEmpty)
        {
            return Result.Failure(LeadErrors.InvalidConversionTarget);
        }

        ConvertedPersonId = personId;
        DraftEnrollmentId = draftEnrollmentId;
        ConvertedAtUtc = convertedAtUtc.ToUniversalTime();
        RaiseDomainEvent(new ProspectConvertedDomainEvent(
            Id, OrganizationId, personId, draftEnrollmentId, ConvertedAtUtc.Value));

        return Result.Success();
    }

    private static bool CanTransition(LeadStatus current, LeadStatus target) =>
        current switch
        {
            LeadStatus.New => target is LeadStatus.Contacted or LeadStatus.Lost or LeadStatus.Dormant,
            LeadStatus.Contacted => target is LeadStatus.Lost or LeadStatus.Dormant,
            LeadStatus.Qualified => target is LeadStatus.AssessmentScheduled or LeadStatus.OfferSent or LeadStatus.Lost or LeadStatus.Dormant,
            LeadStatus.AssessmentScheduled => target is LeadStatus.OfferSent or LeadStatus.Lost or LeadStatus.Dormant,
            LeadStatus.OfferSent => target is LeadStatus.Negotiation or LeadStatus.Won or LeadStatus.Lost or LeadStatus.Dormant,
            LeadStatus.Negotiation => target is LeadStatus.Won or LeadStatus.Lost or LeadStatus.Dormant,
            LeadStatus.Lost or LeadStatus.Dormant => target == LeadStatus.New,
            LeadStatus.Won => false,
            _ => false
        };

    public void SetCreatedAudit(
        DateTimeOffset createdAtUtc,
        UserId? createdByUserId)
    {
        if (CreatedAtUtc != default)
        {
            return;
        }

        CreatedAtUtc = createdAtUtc;
        CreatedByUserId = createdByUserId;
    }

    public void SetModifiedAudit(
        DateTimeOffset modifiedAtUtc,
        UserId? modifiedByUserId)
    {
        LastModifiedAtUtc = modifiedAtUtc;
        LastModifiedByUserId = modifiedByUserId;
    }
}
