using DriveOS.Modules.CRM.Domain.Assessments;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.Modules.CRM.Domain.Offers.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Domain.Offers;

public sealed class CommercialOffer : AggregateRoot<CommercialOfferId>, IAuditableEntity
{
    private readonly List<CommercialOfferLine> _lines = [];
    private readonly List<OfferInteraction> _interactions = [];
    private CommercialOffer() { }

    private CommercialOffer(CommercialOfferId id, OrganizationId organizationId, LeadId leadId,
        AssessmentSessionId assessmentSessionId, int assessmentRevision, BranchId? branchId,
        int version, string trainingCode, string currency, DateTimeOffset validUntilUtc,
        decimal estimatedFundingAmount, string? financingNotes, string? conditions,
        string? internalNotes) : base(id)
    {
        OrganizationId = organizationId;
        LeadId = leadId;
        AssessmentSessionId = assessmentSessionId;
        AssessmentRevision = assessmentRevision;
        BranchId = branchId;
        Version = version;
        TrainingCode = trainingCode;
        Currency = currency;
        ValidUntilUtc = validUntilUtc;
        EstimatedFundingAmount = estimatedFundingAmount;
        FinancingNotes = financingNotes;
        Conditions = conditions;
        InternalNotes = internalNotes;
        Status = CommercialOfferStatus.Draft;
    }

    public OrganizationId OrganizationId { get; private set; }
    public LeadId LeadId { get; private set; }
    public AssessmentSessionId AssessmentSessionId { get; private set; }
    public int AssessmentRevision { get; private set; }
    public BranchId? BranchId { get; private set; }
    public int Version { get; private set; }
    public string TrainingCode { get; private set; } = string.Empty;
    public decimal CatalogAmount { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal Amount { get; private set; }
    public decimal EstimatedFundingAmount { get; private set; }
    public decimal ProspectRemainingAmount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public DateTimeOffset ValidUntilUtc { get; private set; }
    public string? FinancingNotes { get; private set; }
    public string? Conditions { get; private set; }
    public string? InternalNotes { get; private set; }
    public CommercialOfferStatus Status { get; private set; }
    public IReadOnlyCollection<CommercialOfferLine> Lines => _lines;
    public IReadOnlyCollection<OfferInteraction> Interactions => _interactions;
    public DateTimeOffset? SentAtUtc { get; private set; }
    public OfferDeliveryStatus? DeliveryStatus { get; private set; }
    public OfferDeliveryChannel? DeliveryChannel { get; private set; }
    public string? RecipientSnapshotJson { get; private set; }
    public string? DeliverySubject { get; private set; }
    public string? DeliveryMessage { get; private set; }
    public string? DeliveryLanguage { get; private set; }
    public string? DocumentReference { get; private set; }
    public string? AttachmentSnapshotJson { get; private set; }
    public string? SecureLinkTokenHash { get; private set; }
    public DateTimeOffset? SecureLinkExpiresAtUtc { get; private set; }
    public DateTimeOffset? SecureLinkRevokedAtUtc { get; private set; }
    public int DeliveryAttemptCount { get; private set; }
    public DateTimeOffset? ViewedAtUtc { get; private set; }
    public DateTimeOffset? LastViewedAtUtc { get; private set; }
    public int ViewCount { get; private set; }
    public DateTimeOffset? LastContactAtUtc { get; private set; }
    public DateTimeOffset? NextFollowUpAtUtc { get; private set; }
    public DateTimeOffset? DecidedAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<CommercialOffer> Generate(CommercialOfferId id, OrganizationId organizationId,
        LeadId leadId, AssessmentSessionId assessmentSessionId, int assessmentRevision,
        BranchId? branchId, int version, string trainingCode, string currency,
        DateTimeOffset validUntilUtc, DateTimeOffset nowUtc, decimal estimatedFundingAmount,
        string? financingNotes, string? conditions, string? internalNotes,
        IReadOnlyCollection<CommercialOfferLineDraft> lines)
    {
        if (id == CommercialOfferId.Empty || assessmentSessionId == AssessmentSessionId.Empty)
            return Result.Failure<CommercialOffer>(CommercialOfferErrors.InvalidIdentifier);
        if (version < 1 || assessmentRevision < 1)
            return Result.Failure<CommercialOffer>(CommercialOfferErrors.InvalidVersion);
        if (string.IsNullOrWhiteSpace(trainingCode) || trainingCode.Trim().Length > 100)
            return Result.Failure<CommercialOffer>(CommercialOfferErrors.InvalidTraining);
        string normalizedCurrency = currency?.Trim().ToUpperInvariant() ?? string.Empty;
        if (normalizedCurrency.Length != 3 || normalizedCurrency.Any(c => c is < 'A' or > 'Z'))
            return Result.Failure<CommercialOffer>(CommercialOfferErrors.InvalidCurrency);
        if (validUntilUtc <= nowUtc)
            return Result.Failure<CommercialOffer>(CommercialOfferErrors.InvalidValidity);
        if (lines.Count == 0 || estimatedFundingAmount < 0)
            return Result.Failure<CommercialOffer>(CommercialOfferErrors.InvalidAmount);

        var offer = new CommercialOffer(id, organizationId, leadId, assessmentSessionId,
            assessmentRevision, branchId, version, trainingCode.Trim(), normalizedCurrency,
            validUntilUtc.ToUniversalTime(), decimal.Round(estimatedFundingAmount, 2),
            Normalize(financingNotes), Normalize(conditions), Normalize(internalNotes));

        foreach (CommercialOfferLineDraft line in lines)
        {
            Result addResult = offer.AddLine(line);
            if (addResult.IsFailure) return Result.Failure<CommercialOffer>(addResult.Error);
        }
        offer.Recalculate();
        offer.RaiseDomainEvent(new CommercialOfferCreatedDomainEvent(
            offer.Id, offer.OrganizationId, offer.LeadId, offer.Version,
            offer.Amount, offer.Currency));
        offer.AddInteraction(OfferInteractionType.Created, nowUtc, null, null, null);
        return Result.Success(offer);
    }

    private Result AddLine(CommercialOfferLineDraft line)
    {
        if (string.IsNullOrWhiteSpace(line.Description) || line.Description.Trim().Length > 500 ||
            line.Quantity <= 0 || string.IsNullOrWhiteSpace(line.Unit) || line.Unit.Trim().Length > 30 ||
            line.UnitPrice < 0 || line.DiscountAmount < 0 || line.DiscountAmount > line.Quantity * line.UnitPrice ||
            line.TaxRate is < 0 or > 100)
            return Result.Failure(CommercialOfferErrors.InvalidLine);
        if (line.PriceSource == OfferPriceSource.ManualOverride && string.IsNullOrWhiteSpace(line.ManualOverrideReason))
            return Result.Failure(CommercialOfferErrors.ManualOverrideReasonRequired);

        _lines.Add(new CommercialOfferLine(CommercialOfferLineId.New(), Id, line.Type, line.ServiceId,
            line.Description.Trim(), decimal.Round(line.Quantity, 2), line.Unit.Trim(),
            decimal.Round(line.UnitPrice, 2), decimal.Round(line.DiscountAmount, 2),
            decimal.Round(line.TaxRate, 2), line.Mandatory, line.PriceSource,
            Normalize(line.ManualOverrideReason)));
        return Result.Success();
    }

    private void Recalculate()
    {
        CatalogAmount = _lines.Sum(x => decimal.Round(x.Quantity * x.UnitPrice, 2));
        DiscountAmount = _lines.Sum(x => x.DiscountAmount);
        TaxAmount = _lines.Sum(x => x.TaxAmount);
        Amount = _lines.Sum(x => x.TotalAmount);
        ProspectRemainingAmount = Math.Max(0, Amount - EstimatedFundingAmount);
    }

    public Result SubmitForReview() => Transition(CommercialOfferStatus.Draft, CommercialOfferStatus.InternalReview);
    public Result Approve() => Transition(CommercialOfferStatus.InternalReview, CommercialOfferStatus.Approved);
    public Result PrepareDelivery(OfferDeliveryChannel channel, string recipientSnapshotJson,
        string subject, string message, string language, string documentReference,
        string? attachmentSnapshotJson, string? secureLinkTokenHash,
        DateTimeOffset? secureLinkExpiresAtUtc, DateTimeOffset nowUtc)
    {
        if (Status is not (CommercialOfferStatus.Approved or CommercialOfferStatus.Sent) || ValidUntilUtc <= nowUtc)
            return Result.Failure(CommercialOfferErrors.InvalidTransition);
        if (string.IsNullOrWhiteSpace(recipientSnapshotJson) || recipientSnapshotJson == "[]")
            return Result.Failure(CommercialOfferErrors.RecipientRequired);
        if (string.IsNullOrWhiteSpace(subject) || subject.Trim().Length > 250 ||
            string.IsNullOrWhiteSpace(message) || message.Trim().Length > 8000)
            return Result.Failure(CommercialOfferErrors.InvalidMessage);
        string normalizedLanguage = language?.Trim().ToLowerInvariant() ?? string.Empty;
        if (normalizedLanguage.Length is < 2 or > 10)
            return Result.Failure(CommercialOfferErrors.InvalidLanguage);
        if (string.IsNullOrWhiteSpace(documentReference) || documentReference.Trim().Length > 500)
            return Result.Failure(CommercialOfferErrors.InvalidMessage);
        if (channel.RequiresSecureLink() &&
            (string.IsNullOrWhiteSpace(secureLinkTokenHash) || secureLinkExpiresAtUtc is null || secureLinkExpiresAtUtc <= nowUtc))
            return Result.Failure(CommercialOfferErrors.SecureLinkRequired);

        DeliveryChannel = channel;
        RecipientSnapshotJson = recipientSnapshotJson;
        DeliverySubject = subject.Trim();
        DeliveryMessage = message.Trim();
        DeliveryLanguage = normalizedLanguage;
        DocumentReference = documentReference.Trim();
        AttachmentSnapshotJson = Normalize(attachmentSnapshotJson);
        SecureLinkTokenHash = Normalize(secureLinkTokenHash);
        SecureLinkExpiresAtUtc = secureLinkExpiresAtUtc?.ToUniversalTime();
        SecureLinkRevokedAtUtc = null;
        DeliveryAttemptCount++;
        DeliveryStatus = OfferDeliveryStatus.Ready;
        RaiseDomainEvent(new CommercialOfferDeliveryPreparedDomainEvent(
            Id, OrganizationId, LeadId, channel,
            CountRecipients(recipientSnapshotJson), normalizedLanguage, SecureLinkExpiresAtUtc));
        return Result.Success();
    }

    public Result MarkSent(DateTimeOffset nowUtc)
    {
        if (Status is not (CommercialOfferStatus.Approved or CommercialOfferStatus.Sent) || DeliveryStatus is not (OfferDeliveryStatus.Ready or OfferDeliveryStatus.Sending))
            return Result.Failure(CommercialOfferErrors.InvalidTransition);
        Status = CommercialOfferStatus.Sent;
        DeliveryStatus = OfferDeliveryStatus.Sent;
        SentAtUtc = nowUtc.ToUniversalTime();
        LastContactAtUtc = SentAtUtc;
        AddInteraction(OfferInteractionType.Sent, SentAtUtc.Value, null, null, null);
        RaiseDomainEvent(new CommercialOfferSentDomainEvent(Id, OrganizationId, LeadId, SentAtUtc.Value));
        return Result.Success();
    }

    public Result RevokeSecureLink(DateTimeOffset nowUtc)
    {
        if (SecureLinkExpiresAtUtc is null) return Result.Failure(CommercialOfferErrors.SecureLinkRequired);
        if (SecureLinkRevokedAtUtc is not null) return Result.Failure(CommercialOfferErrors.SecureLinkAlreadyRevoked);
        SecureLinkRevokedAtUtc = nowUtc.ToUniversalTime();
        DeliveryStatus = OfferDeliveryStatus.LinkExpired;
        RaiseDomainEvent(new CommercialOfferSecureLinkRevokedDomainEvent(
            Id, OrganizationId, LeadId, SecureLinkRevokedAtUtc.Value));
        return Result.Success();
    }
    public Result Accept(DateTimeOffset nowUtc)
    {
        Result result = Decide(CommercialOfferStatus.Accepted, nowUtc);
        if (result.IsSuccess)
        {
            AddInteraction(OfferInteractionType.Accepted, DecidedAtUtc!.Value, null, null, null);
            RaiseDomainEvent(new CommercialOfferAcceptedDomainEvent(Id, OrganizationId, LeadId, DecidedAtUtc.Value));
        }
        return result;
    }
    public Result Reject(DateTimeOffset nowUtc)
    {
        Result result = Decide(CommercialOfferStatus.Rejected, nowUtc);
        if (result.IsSuccess)
        {
            AddInteraction(OfferInteractionType.Rejected, DecidedAtUtc!.Value, null, null, null);
            RaiseDomainEvent(new CommercialOfferRejectedDomainEvent(Id, OrganizationId, LeadId, DecidedAtUtc.Value));
        }
        return result;
    }
    public Result Supersede(DateTimeOffset nowUtc) { Status = CommercialOfferStatus.Superseded; DecidedAtUtc = nowUtc.ToUniversalTime(); return Result.Success(); }
    public Result RecordView(DateTimeOffset nowUtc)
    {
        if (Status is not (CommercialOfferStatus.Sent or CommercialOfferStatus.Viewed or CommercialOfferStatus.Negotiation) ||
            SecureLinkRevokedAtUtc is not null || SecureLinkExpiresAtUtc is null || SecureLinkExpiresAtUtc <= nowUtc)
            return Result.Failure(CommercialOfferErrors.SecureLinkExpired);

        DateTimeOffset viewedAt = nowUtc.ToUniversalTime();
        ViewedAtUtc ??= viewedAt;
        LastViewedAtUtc = viewedAt;
        ViewCount++;
        if (Status == CommercialOfferStatus.Sent) Status = CommercialOfferStatus.Viewed;
        DeliveryStatus = OfferDeliveryStatus.Viewed;
        AddInteraction(OfferInteractionType.Viewed, viewedAt, null, null, null);
        RaiseDomainEvent(new CommercialOfferViewedDomainEvent(Id, OrganizationId, LeadId, ViewCount, viewedAt));
        return Result.Success();
    }

    public Result RecordExchange(OfferInteractionType type, string summary,
        string? metadataJson, UserId? actorUserId, DateTimeOffset nowUtc)
    {
        if (type is not (OfferInteractionType.QuestionReceived or OfferInteractionType.ModificationRequested or
            OfferInteractionType.FollowUpCompleted) || string.IsNullOrWhiteSpace(summary) || summary.Trim().Length > 4000)
            return Result.Failure(CommercialOfferErrors.InvalidInteraction);
        if (Status is not (CommercialOfferStatus.Sent or CommercialOfferStatus.Viewed or CommercialOfferStatus.Negotiation))
            return Result.Failure(CommercialOfferErrors.InvalidTransition);

        Status = CommercialOfferStatus.Negotiation;
        LastContactAtUtc = nowUtc.ToUniversalTime();
        OfferInteraction interaction = AddInteraction(type, LastContactAtUtc.Value, actorUserId,
            summary.Trim(), Normalize(metadataJson));
        if (type == OfferInteractionType.ModificationRequested)
            RaiseDomainEvent(new CommercialOfferModificationRequestedDomainEvent(Id, OrganizationId, LeadId, interaction.Id));
        return Result.Success();
    }

    public Result ScheduleFollowUp(DateTimeOffset nextFollowUpAtUtc, string? note,
        UserId? actorUserId, DateTimeOffset nowUtc)
    {
        if (nextFollowUpAtUtc <= nowUtc || Status is CommercialOfferStatus.Accepted or CommercialOfferStatus.Rejected or
            CommercialOfferStatus.Withdrawn or CommercialOfferStatus.Expired or CommercialOfferStatus.Superseded)
            return Result.Failure(CommercialOfferErrors.FollowUpDateRequired);
        NextFollowUpAtUtc = nextFollowUpAtUtc.ToUniversalTime();
        AddInteraction(OfferInteractionType.FollowUpScheduled, nowUtc, actorUserId,
            Normalize(note), $"{{\"nextFollowUpAtUtc\":\"{NextFollowUpAtUtc:O}\"}}");
        return Result.Success();
    }

    public Result Withdraw(string reason, UserId? actorUserId, DateTimeOffset nowUtc)
    {
        if (Status is CommercialOfferStatus.Accepted or CommercialOfferStatus.Rejected or
            CommercialOfferStatus.Withdrawn or CommercialOfferStatus.Expired or CommercialOfferStatus.Superseded ||
            string.IsNullOrWhiteSpace(reason))
            return Result.Failure(CommercialOfferErrors.InvalidTransition);
        Status = CommercialOfferStatus.Withdrawn;
        DecidedAtUtc = nowUtc.ToUniversalTime();
        AddInteraction(OfferInteractionType.Withdrawn, DecidedAtUtc.Value, actorUserId, reason.Trim(), null);
        RaiseDomainEvent(new CommercialOfferWithdrawnDomainEvent(Id, OrganizationId, LeadId, DecidedAtUtc.Value));
        return Result.Success();
    }

    public Result Expire(DateTimeOffset nowUtc)
    {
        if (ValidUntilUtc > nowUtc || Status is CommercialOfferStatus.Accepted or CommercialOfferStatus.Rejected or CommercialOfferStatus.Withdrawn)
            return Result.Failure(CommercialOfferErrors.InvalidTransition);
        if (Status == CommercialOfferStatus.Expired) return Result.Failure(CommercialOfferErrors.OfferAlreadyExpired);
        Status = CommercialOfferStatus.Expired;
        DecidedAtUtc = nowUtc.ToUniversalTime();
        AddInteraction(OfferInteractionType.Expired, DecidedAtUtc.Value, null, null, null);
        return Result.Success();
    }
    private Result Transition(CommercialOfferStatus from, CommercialOfferStatus to) { if (Status != from) return Result.Failure(CommercialOfferErrors.InvalidTransition); Status = to; return Result.Success(); }
    private Result Decide(CommercialOfferStatus target, DateTimeOffset nowUtc) { if (Status is not (CommercialOfferStatus.Sent or CommercialOfferStatus.Viewed or CommercialOfferStatus.Negotiation) || ValidUntilUtc < nowUtc) return Result.Failure(CommercialOfferErrors.InvalidTransition); Status = target; DecidedAtUtc = nowUtc.ToUniversalTime(); return Result.Success(); }
    public void SetCreatedAudit(DateTimeOffset at, UserId? by) { if (CreatedAtUtc == default) { CreatedAtUtc = at; CreatedByUserId = by; } }
    public void SetModifiedAudit(DateTimeOffset at, UserId? by) { LastModifiedAtUtc = at; LastModifiedByUserId = by; }
    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static int CountRecipients(string snapshot) => snapshot.Count(x => x == '{');
    private OfferInteraction AddInteraction(OfferInteractionType type, DateTimeOffset occurredAtUtc,
        UserId? actorUserId, string? summary, string? metadataJson)
    {
        var interaction = new OfferInteraction(OfferInteractionId.New(), Id, type,
            occurredAtUtc, actorUserId, summary, metadataJson);
        _interactions.Add(interaction);
        return interaction;
    }
}

public sealed record CommercialOfferLineDraft(OfferLineType Type, ServiceId? ServiceId,
    string Description, decimal Quantity, string Unit, decimal UnitPrice,
    decimal DiscountAmount, decimal TaxRate, bool Mandatory,
    OfferPriceSource PriceSource, string? ManualOverrideReason);
