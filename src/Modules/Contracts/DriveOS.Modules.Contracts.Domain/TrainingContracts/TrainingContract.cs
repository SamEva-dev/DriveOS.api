using DriveOS.Modules.Contracts.Domain.ContractAmendments;
using DriveOS.Modules.Contracts.Domain.TrainingContracts.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Contracts.Domain.TrainingContracts;

public sealed class TrainingContract : AggregateRoot<TrainingContractId>, IAuditableEntity
{
    private readonly List<TrainingContractParty> _parties = [];
    private readonly List<TrainingContractVersion> _versions = [];
    private readonly List<TrainingContractSignatory> _signatories = [];

    private TrainingContract() { }

    private TrainingContract(
        TrainingContractId id,
        OrganizationId organizationId,
        BranchId branchId,
        PersonId studentId,
        CommercialOfferId sourceOfferId,
        int sourceOfferVersion,
        string contractNumber,
        DateOnly startDate,
        DateOnly? endDate,
        decimal totalAmount,
        string currency,
        TrainingContractTermsSnapshot termsSnapshot)
        : base(id)
    {
        OrganizationId = organizationId;
        BranchId = branchId;
        StudentId = studentId;
        SourceOfferId = sourceOfferId;
        SourceOfferVersion = sourceOfferVersion;
        ContractNumber = contractNumber;
        StartDate = startDate;
        EndDate = endDate;
        TotalAmount = totalAmount;
        Currency = currency;
        TermsSnapshot = termsSnapshot;
        CurrentVersionNumber = 1;
        Status = TrainingContractStatus.Draft;
    }

    public OrganizationId OrganizationId { get; private set; }
    public BranchId BranchId { get; private set; }
    public PersonId StudentId { get; private set; }
    public CommercialOfferId SourceOfferId { get; private set; }
    public int SourceOfferVersion { get; private set; }
    public string ContractNumber { get; private set; } = string.Empty;
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public TrainingContractTermsSnapshot TermsSnapshot { get; private set; } = null!;
    public int CurrentVersionNumber { get; private set; }
    public TrainingContractStatus Status { get; private set; }
    public string? GeneratedDocumentReference { get; private set; }
    public string? GeneratedDocumentFileName { get; private set; }
    public string? GeneratedDocumentContentType { get; private set; }
    public string? GeneratedDocumentSha256 { get; private set; }
    public int? GeneratedDocumentVersionNumber { get; private set; }
    public DateTimeOffset? GeneratedAtUtc { get; private set; }
    public UserId? GeneratedByUserId { get; private set; }
    public DateTimeOffset? ActivatedAtUtc { get; private set; }
    public UserId? ActivatedByUserId { get; private set; }
    public string? SuspensionReason { get; private set; }
    public DateOnly? SuspensionEffectiveDate { get; private set; }
    public DateOnly? SuspensionExpectedResumeDate { get; private set; }
    public DateTimeOffset? SuspendedAtUtc { get; private set; }
    public UserId? SuspendedByUserId { get; private set; }
    public string? TerminationReason { get; private set; }
    public DateOnly? TerminationEffectiveDate { get; private set; }
    public DateTimeOffset? TerminatedAtUtc { get; private set; }
    public UserId? TerminatedByUserId { get; private set; }
    public string? CompletionNote { get; private set; }
    public DateOnly? CompletionEffectiveDate { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }
    public UserId? CompletedByUserId { get; private set; }
    public DateOnly? ExpirationEffectiveDate { get; private set; }
    public DateTimeOffset? ExpiredAtUtc { get; private set; }
    public UserId? ExpiredByUserId { get; private set; }
    public IReadOnlyCollection<TrainingContractParty> Parties => _parties.AsReadOnly();
    public IReadOnlyCollection<TrainingContractVersion> Versions => _versions.AsReadOnly();
    public IReadOnlyCollection<TrainingContractSignatory> Signatories => _signatories.AsReadOnly();
    public TrainingContractVersion CurrentVersion =>
        _versions.Single(version => version.VersionNumber == CurrentVersionNumber);

    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public bool IsSignedOrBeyond => Status is
        TrainingContractStatus.Signed or
        TrainingContractStatus.Active or
        TrainingContractStatus.Amended or
        TrainingContractStatus.Suspended or
        TrainingContractStatus.Terminated or
        TrainingContractStatus.Completed or
        TrainingContractStatus.Expired;

    public bool CanCreateRevision => Status is
        TrainingContractStatus.Draft or
        TrainingContractStatus.Generated;

    public bool CanGenerate => Status == TrainingContractStatus.Draft;
    public bool CanSendForSignature => Status == TrainingContractStatus.Generated;
    public bool CanManageSignatories => Status is TrainingContractStatus.Draft or TrainingContractStatus.Generated;
    public bool CanActivate => Status == TrainingContractStatus.Signed;
    public bool CanAmend => Status is TrainingContractStatus.Active or TrainingContractStatus.Amended;
    public bool CanSuspend => Status is TrainingContractStatus.Active or TrainingContractStatus.Amended;
    public bool CanTerminate => Status is TrainingContractStatus.Active or TrainingContractStatus.Amended or TrainingContractStatus.Suspended;
    public bool CanComplete => Status is TrainingContractStatus.Active or TrainingContractStatus.Amended;
    public bool CanExpire => Status is TrainingContractStatus.Signed or TrainingContractStatus.Active or TrainingContractStatus.Amended or TrainingContractStatus.Suspended;

    public static Result<TrainingContract> CreateDraft(
        TrainingContractId id,
        OrganizationId organizationId,
        BranchId branchId,
        PersonId studentId,
        CommercialOfferId sourceOfferId,
        int sourceOfferVersion,
        string contractNumber,
        DateOnly startDate,
        DateOnly? endDate,
        decimal totalAmount,
        string currency,
        TrainingContractTermsSnapshot termsSnapshot,
        IReadOnlyCollection<TrainingContractParty> parties)
    {
        Result validation = ValidateContractData(
            id,
            organizationId,
            branchId,
            studentId,
            sourceOfferId,
            sourceOfferVersion,
            contractNumber,
            startDate,
            endDate,
            totalAmount,
            currency,
            termsSnapshot,
            parties);

        if (validation.IsFailure)
            return Result.Failure<TrainingContract>(validation.Error);

        string normalizedNumber = contractNumber.Trim().ToUpperInvariant();
        string normalizedCurrency = currency.Trim().ToUpperInvariant();
        decimal normalizedAmount = decimal.Round(totalAmount, 2);

        var contract = new TrainingContract(
            id,
            organizationId,
            branchId,
            studentId,
            sourceOfferId,
            sourceOfferVersion,
            normalizedNumber,
            startDate,
            endDate,
            normalizedAmount,
            normalizedCurrency,
            termsSnapshot);

        contract._parties.AddRange(parties);

        Result<TrainingContractVersion> versionResult = TrainingContractVersion.Create(
            contract.Id,
            1,
            sourceOfferId,
            sourceOfferVersion,
            startDate,
            endDate,
            normalizedAmount,
            normalizedCurrency,
            termsSnapshot,
            parties,
            revisionReason: null,
            createdByUserId: null,
            createdAtUtc: default);

        if (versionResult.IsFailure)
            return Result.Failure<TrainingContract>(versionResult.Error);

        contract._versions.Add(versionResult.Value);

        contract.RaiseDomainEvent(new TrainingContractDraftCreatedDomainEvent(
            contract.Id,
            contract.OrganizationId,
            contract.BranchId,
            contract.StudentId,
            contract.SourceOfferId,
            contract.SourceOfferVersion,
            contract.ContractNumber,
            contract.TotalAmount,
            contract.Currency));

        return Result.Success(contract);
    }

    public Result<TrainingContractVersion> CreateRevision(
        CommercialOfferId sourceOfferId,
        int sourceOfferVersion,
        DateOnly startDate,
        DateOnly? endDate,
        decimal totalAmount,
        string currency,
        TrainingContractTermsSnapshot termsSnapshot,
        IReadOnlyCollection<TrainingContractParty> parties,
        string revisionReason,
        UserId? createdByUserId,
        DateTimeOffset createdAtUtc)
    {
        if (!CanCreateRevision)
            return Result.Failure<TrainingContractVersion>(TrainingContractErrors.RevisionLocked);

        Result validation = ValidateContractData(
            Id,
            OrganizationId,
            BranchId,
            StudentId,
            sourceOfferId,
            sourceOfferVersion,
            ContractNumber,
            startDate,
            endDate,
            totalAmount,
            currency,
            termsSnapshot,
            parties);

        if (validation.IsFailure)
            return Result.Failure<TrainingContractVersion>(validation.Error);

        int nextVersionNumber = CurrentVersionNumber + 1;
        Result<TrainingContractVersion> versionResult = TrainingContractVersion.Create(
            Id,
            nextVersionNumber,
            sourceOfferId,
            sourceOfferVersion,
            startDate,
            endDate,
            totalAmount,
            currency,
            termsSnapshot,
            parties,
            revisionReason,
            createdByUserId,
            createdAtUtc);

        if (versionResult.IsFailure)
            return versionResult;

        TrainingContractVersion version = versionResult.Value;
        _versions.Add(version);

        SourceOfferId = version.SourceOfferId;
        SourceOfferVersion = version.SourceOfferVersion;
        StartDate = version.StartDate;
        EndDate = version.EndDate;
        TotalAmount = version.TotalAmount;
        Currency = version.Currency;
        TermsSnapshot = version.TermsSnapshot;
        CurrentVersionNumber = version.VersionNumber;
        Status = TrainingContractStatus.Draft;
        GeneratedDocumentReference = null;
        GeneratedDocumentFileName = null;
        GeneratedDocumentContentType = null;
        GeneratedDocumentSha256 = null;
        GeneratedDocumentVersionNumber = null;
        GeneratedAtUtc = null;
        GeneratedByUserId = null;

        _parties.Clear();
        _parties.AddRange(parties);

        RaiseDomainEvent(new TrainingContractVersionCreatedDomainEvent(
            Id,
            version.Id,
            version.VersionNumber,
            version.SourceOfferId,
            version.SourceOfferVersion,
            version.RevisionReason!));

        return Result.Success(version);
    }

    public Result MarkGenerated(
        string documentReference,
        string fileName,
        string contentType,
        string sha256,
        UserId generatedByUserId,
        DateTimeOffset generatedAtUtc)
    {
        if (!CanGenerate)
            return Result.Failure(TrainingContractErrors.GenerationNotAllowed);

        if (string.IsNullOrWhiteSpace(documentReference) ||
            string.IsNullOrWhiteSpace(fileName) ||
            string.IsNullOrWhiteSpace(contentType) ||
            string.IsNullOrWhiteSpace(sha256) ||
            sha256.Length != 64)
            return Result.Failure(TrainingContractErrors.InvalidGeneratedDocument);

        GeneratedDocumentReference = documentReference.Trim();
        GeneratedDocumentFileName = fileName.Trim();
        GeneratedDocumentContentType = contentType.Trim();
        GeneratedDocumentSha256 = sha256.Trim().ToUpperInvariant();
        GeneratedDocumentVersionNumber = CurrentVersionNumber;
        GeneratedAtUtc = generatedAtUtc.ToUniversalTime();
        GeneratedByUserId = generatedByUserId;
        Status = TrainingContractStatus.Generated;

        RaiseDomainEvent(new TrainingContractGeneratedDomainEvent(
            Id, CurrentVersionNumber, GeneratedDocumentSha256, GeneratedAtUtc.Value));

        return Result.Success();
    }

    public Result<TrainingContractSignatory> AddSignatory(
        TrainingContractSignatoryKind kind, PersonId personId, OrganizationId? representedOrganizationId,
        string displayName, int signingOrder, bool isRequired, string? authorityReference)
    {
        if (!CanManageSignatories) return Result.Failure<TrainingContractSignatory>(TrainingContractErrors.SignatoryManagementLocked);
        if (_signatories.Any(x => x.PersonId == personId && x.Kind == kind))
            return Result.Failure<TrainingContractSignatory>(TrainingContractErrors.DuplicateSignatory);
        Result<TrainingContractSignatory> result = TrainingContractSignatory.Create(
            Id, kind, personId, representedOrganizationId, displayName, signingOrder, isRequired, authorityReference);
        if (result.IsFailure) return result;
        _signatories.Add(result.Value);
        RaiseDomainEvent(new TrainingContractSignatoryAddedDomainEvent(Id, result.Value.Id, personId, kind.ToString(), signingOrder, isRequired));
        return result;
    }

    public Result UpdateSignatory(TrainingContractSignatoryId signatoryId, int signingOrder, bool isRequired, string displayName, string? authorityReference)
    {
        if (!CanManageSignatories) return Result.Failure(TrainingContractErrors.SignatoryManagementLocked);
        TrainingContractSignatory? signatory = _signatories.SingleOrDefault(x => x.Id == signatoryId);
        if (signatory is null) return Result.Failure(TrainingContractErrors.SignatoryNotFound);
        return signatory.Update(signingOrder, isRequired, displayName, authorityReference);
    }

    public Result RemoveSignatory(TrainingContractSignatoryId signatoryId)
    {
        if (!CanManageSignatories) return Result.Failure(TrainingContractErrors.SignatoryManagementLocked);
        TrainingContractSignatory? signatory = _signatories.SingleOrDefault(x => x.Id == signatoryId);
        if (signatory is null) return Result.Failure(TrainingContractErrors.SignatoryNotFound);
        _signatories.Remove(signatory);
        RaiseDomainEvent(new TrainingContractSignatoryRemovedDomainEvent(Id, signatoryId));
        return Result.Success();
    }

    public Result DecideSignatoryAuthority(TrainingContractSignatoryId signatoryId, bool approved, string? reason, UserId actorUserId, DateTimeOffset decidedAtUtc)
    {
        if (!CanManageSignatories) return Result.Failure(TrainingContractErrors.SignatoryManagementLocked);
        TrainingContractSignatory? signatory = _signatories.SingleOrDefault(x => x.Id == signatoryId);
        if (signatory is null) return Result.Failure(TrainingContractErrors.SignatoryNotFound);
        Result result = approved
            ? signatory.VerifyAuthority(actorUserId, decidedAtUtc)
            : signatory.RejectAuthority(reason ?? string.Empty, actorUserId, decidedAtUtc);
        if (result.IsSuccess) RaiseDomainEvent(new TrainingContractSignatoryAuthorityDecidedDomainEvent(Id, signatoryId, approved, actorUserId, decidedAtUtc.ToUniversalTime()));
        return result;
    }


    public Result MarkSentForSignature(SignatureProcessId signatureProcessId, UserId actorUserId, DateTimeOffset sentAtUtc)
    {
        if (!CanSendForSignature) return Result.Failure(TrainingContractErrors.SendForSignatureNotAllowed);
        if (signatureProcessId.IsEmpty) return Result.Failure(TrainingContractErrors.InvalidSignatureProcess);
        if (GeneratedDocumentVersionNumber != CurrentVersionNumber || string.IsNullOrWhiteSpace(GeneratedDocumentReference) || string.IsNullOrWhiteSpace(GeneratedDocumentSha256))
            return Result.Failure(TrainingContractErrors.GeneratedDocumentOutdated);
        if (_signatories.Count == 0 || !_signatories.Any(x => x.IsRequired))
            return Result.Failure(TrainingContractErrors.RequiredSignatoryMissing);
        if (_signatories.Where(x => x.IsRequired).Any(x => x.AuthorityStatus != SignatoryAuthorityStatus.Verified || x.Status != TrainingContractSignatoryStatus.Ready))
            return Result.Failure(TrainingContractErrors.SignatoryNotReady);

        Status = TrainingContractStatus.SentForSignature;
        RaiseDomainEvent(new TrainingContractSentForSignatureDomainEvent(Id, signatureProcessId, CurrentVersionNumber, actorUserId, sentAtUtc.ToUniversalTime()));
        return Result.Success();
    }

    public Result RecordSignatorySignature(
        TrainingContractSignatoryId signatoryId,
        SignatureEvidenceId evidenceId,
        UserId actorUserId,
        DateTimeOffset signedAtUtc)
    {
        if (evidenceId.IsEmpty || actorUserId.IsEmpty || signedAtUtc == default)
            return Result.Failure(TrainingContractErrors.InvalidSignatureEvidence);

        TrainingContractSignatory? signatory = _signatories.SingleOrDefault(x => x.Id == signatoryId);
        if (signatory is null)
            return Result.Failure(TrainingContractErrors.SignatoryNotFound);
        if (signatory.Status == TrainingContractSignatoryStatus.Signed && Status == TrainingContractStatus.Signed)
            return Result.Success();
        if (Status is not (TrainingContractStatus.SentForSignature or TrainingContractStatus.PartiallySigned))
            return Result.Failure(TrainingContractErrors.SignatureRecordingNotAllowed);
        if (signatory.Status == TrainingContractSignatoryStatus.Signed)
            return Result.Success();

        Result signed = signatory.MarkSigned(signedAtUtc);
        if (signed.IsFailure)
            return signed;

        bool allRequiredSigned = _signatories.Where(x => x.IsRequired)
            .All(x => x.Status == TrainingContractSignatoryStatus.Signed);

        Status = allRequiredSigned ? TrainingContractStatus.Signed : TrainingContractStatus.PartiallySigned;

        RaiseDomainEvent(new TrainingContractSignatorySignedDomainEvent(
            Id, signatoryId, evidenceId, actorUserId, signedAtUtc.ToUniversalTime()));

        if (allRequiredSigned)
        {
            RaiseDomainEvent(new TrainingContractSignedDomainEvent(
                Id, CurrentVersionNumber, actorUserId, signedAtUtc.ToUniversalTime()));
        }

        return Result.Success();
    }

    public Result<int> ApplySignedAmendment(
        ContractAmendment amendment,
        UserId actorUserId,
        DateTimeOffset appliedAtUtc)
    {
        if (!CanAmend)
            return Result.Failure<int>(TrainingContractErrors.AmendmentNotAllowed);
        if (amendment is null || amendment.ContractId != Id || amendment.OrganizationId != OrganizationId)
            return Result.Failure<int>(TrainingContractErrors.InvalidAmendment);
        if (amendment.Status != ContractAmendmentStatus.Signed)
            return Result.Failure<int>(TrainingContractErrors.AmendmentNotSigned);
        if (amendment.BaseContractVersionNumber != CurrentVersionNumber)
            return Result.Failure<int>(ContractAmendmentErrors.BaseVersionChanged);
        if (amendment.EffectiveDate > DateOnly.FromDateTime(appliedAtUtc.UtcDateTime))
            return Result.Failure<int>(TrainingContractErrors.AmendmentNotEffectiveYet);

        int nextVersionNumber = CurrentVersionNumber + 1;
        Result<TrainingContractVersion> versionResult = TrainingContractVersion.Create(
            Id,
            nextVersionNumber,
            SourceOfferId,
            SourceOfferVersion,
            amendment.StartDate,
            amendment.EndDate,
            amendment.TotalAmount,
            amendment.Currency,
            amendment.TermsSnapshot,
            _parties.ToArray(),
            $"Amendment #{amendment.AmendmentNumber}: {amendment.Reason}",
            actorUserId,
            appliedAtUtc);

        if (versionResult.IsFailure)
            return Result.Failure<int>(versionResult.Error);

        TrainingContractVersion version = versionResult.Value;
        _versions.Add(version);
        StartDate = version.StartDate;
        EndDate = version.EndDate;
        TotalAmount = version.TotalAmount;
        Currency = version.Currency;
        TermsSnapshot = version.TermsSnapshot;
        CurrentVersionNumber = version.VersionNumber;
        Status = TrainingContractStatus.Amended;
        GeneratedDocumentReference = null;
        GeneratedDocumentFileName = null;
        GeneratedDocumentContentType = null;
        GeneratedDocumentSha256 = null;
        GeneratedDocumentVersionNumber = null;
        GeneratedAtUtc = null;
        GeneratedByUserId = null;

        RaiseDomainEvent(new TrainingContractAmendedDomainEvent(
            Id, amendment.Id, amendment.AmendmentNumber, version.VersionNumber, actorUserId, appliedAtUtc.ToUniversalTime()));

        return Result.Success(version.VersionNumber);
    }

    public Result Activate(UserId actorUserId, DateTimeOffset activatedAtUtc)
    {
        if (!CanActivate)
            return Result.Failure(TrainingContractErrors.ActivationNotAllowed);

        if (actorUserId.IsEmpty || activatedAtUtc == default)
            return Result.Failure(TrainingContractErrors.InvalidActivation);

        DateOnly activationDate = DateOnly.FromDateTime(activatedAtUtc.UtcDateTime);
        if (activationDate < StartDate)
            return Result.Failure(TrainingContractErrors.ActivationBeforeStartDate);

        if (EndDate.HasValue && activationDate > EndDate.Value)
            return Result.Failure(TrainingContractErrors.ActivationAfterEndDate);

        Status = TrainingContractStatus.Active;
        ActivatedAtUtc = activatedAtUtc.ToUniversalTime();
        ActivatedByUserId = actorUserId;

        RaiseDomainEvent(new TrainingContractActivatedDomainEvent(
            Id, CurrentVersionNumber, actorUserId, ActivatedAtUtc.Value));

        return Result.Success();
    }

    public Result Suspend(
        string reason,
        DateOnly effectiveDate,
        DateOnly? expectedResumeDate,
        UserId actorUserId,
        DateTimeOffset suspendedAtUtc)
    {
        if (!CanSuspend)
            return Result.Failure(TrainingContractErrors.SuspensionNotAllowed);

        string normalizedReason = reason?.Trim() ?? string.Empty;
        if (normalizedReason.Length is < 10 or > 500 || actorUserId.IsEmpty || suspendedAtUtc == default)
            return Result.Failure(TrainingContractErrors.InvalidSuspension);

        DateOnly today = DateOnly.FromDateTime(suspendedAtUtc.UtcDateTime);
        if (effectiveDate != today)
            return Result.Failure(TrainingContractErrors.SuspensionEffectiveDateMustBeToday);
        if (EndDate.HasValue && effectiveDate > EndDate.Value)
            return Result.Failure(TrainingContractErrors.SuspensionAfterContractEnd);
        if (expectedResumeDate.HasValue && expectedResumeDate.Value <= effectiveDate)
            return Result.Failure(TrainingContractErrors.InvalidSuspensionResumeDate);
        if (EndDate.HasValue && expectedResumeDate.HasValue && expectedResumeDate.Value > EndDate.Value)
            return Result.Failure(TrainingContractErrors.SuspensionResumeAfterContractEnd);

        Status = TrainingContractStatus.Suspended;
        SuspensionReason = normalizedReason;
        SuspensionEffectiveDate = effectiveDate;
        SuspensionExpectedResumeDate = expectedResumeDate;
        SuspendedAtUtc = suspendedAtUtc.ToUniversalTime();
        SuspendedByUserId = actorUserId;

        RaiseDomainEvent(new TrainingContractSuspendedDomainEvent(
            Id, CurrentVersionNumber, effectiveDate, expectedResumeDate, normalizedReason, actorUserId, SuspendedAtUtc.Value));

        return Result.Success();
    }

    public Result Terminate(
        string reason,
        DateOnly effectiveDate,
        UserId actorUserId,
        DateTimeOffset terminatedAtUtc)
    {
        if (!CanTerminate)
            return Result.Failure(TrainingContractErrors.TerminationNotAllowed);

        string normalizedReason = reason?.Trim() ?? string.Empty;
        if (normalizedReason.Length is < 10 or > 500 || actorUserId.IsEmpty || terminatedAtUtc == default)
            return Result.Failure(TrainingContractErrors.InvalidTermination);

        DateOnly today = DateOnly.FromDateTime(terminatedAtUtc.UtcDateTime);
        if (effectiveDate != today)
            return Result.Failure(TrainingContractErrors.TerminationEffectiveDateMustBeToday);

        if (effectiveDate < StartDate)
            return Result.Failure(TrainingContractErrors.TerminationBeforeContractStart);

        Status = TrainingContractStatus.Terminated;
        TerminationReason = normalizedReason;
        TerminationEffectiveDate = effectiveDate;
        TerminatedAtUtc = terminatedAtUtc.ToUniversalTime();
        TerminatedByUserId = actorUserId;

        RaiseDomainEvent(new TrainingContractTerminatedDomainEvent(
            Id, CurrentVersionNumber, effectiveDate, normalizedReason, actorUserId, TerminatedAtUtc.Value));

        return Result.Success();
    }

    public Result Complete(
        string note,
        DateOnly effectiveDate,
        UserId actorUserId,
        DateTimeOffset completedAtUtc)
    {
        if (!CanComplete)
            return Result.Failure(TrainingContractErrors.CompletionNotAllowed);

        string normalizedNote = note?.Trim() ?? string.Empty;
        if (normalizedNote.Length is < 10 or > 500 || actorUserId.IsEmpty || completedAtUtc == default)
            return Result.Failure(TrainingContractErrors.InvalidCompletion);

        DateOnly today = DateOnly.FromDateTime(completedAtUtc.UtcDateTime);
        if (effectiveDate != today)
            return Result.Failure(TrainingContractErrors.CompletionEffectiveDateMustBeToday);
        if (effectiveDate < StartDate)
            return Result.Failure(TrainingContractErrors.CompletionBeforeContractStart);
        if (EndDate.HasValue && effectiveDate > EndDate.Value)
            return Result.Failure(TrainingContractErrors.CompletionAfterContractEnd);

        Status = TrainingContractStatus.Completed;
        CompletionNote = normalizedNote;
        CompletionEffectiveDate = effectiveDate;
        CompletedAtUtc = completedAtUtc.ToUniversalTime();
        CompletedByUserId = actorUserId;

        RaiseDomainEvent(new TrainingContractCompletedDomainEvent(
            Id, CurrentVersionNumber, effectiveDate, normalizedNote, actorUserId, CompletedAtUtc.Value));

        return Result.Success();
    }

    public Result Expire(UserId actorUserId, DateTimeOffset expiredAtUtc)
    {
        if (!CanExpire)
            return Result.Failure(TrainingContractErrors.ExpirationNotAllowed);
        if (actorUserId.IsEmpty || expiredAtUtc == default)
            return Result.Failure(TrainingContractErrors.InvalidExpiration);
        if (!EndDate.HasValue)
            return Result.Failure(TrainingContractErrors.ExpirationRequiresEndDate);

        DateOnly today = DateOnly.FromDateTime(expiredAtUtc.UtcDateTime);
        if (today <= EndDate.Value)
            return Result.Failure(TrainingContractErrors.ContractNotExpiredYet);

        Status = TrainingContractStatus.Expired;
        ExpirationEffectiveDate = EndDate.Value;
        ExpiredAtUtc = expiredAtUtc.ToUniversalTime();
        ExpiredByUserId = actorUserId;

        RaiseDomainEvent(new TrainingContractExpiredDomainEvent(
            Id, CurrentVersionNumber, EndDate.Value, actorUserId, ExpiredAtUtc.Value));

        return Result.Success();
    }

    public void SetCreatedAudit(DateTimeOffset createdAtUtc, UserId? createdByUserId)
    {
        if (CreatedAtUtc != default)
            return;

        CreatedAtUtc = createdAtUtc.ToUniversalTime();
        CreatedByUserId = createdByUserId;

        foreach (TrainingContractVersion version in _versions)
            version.StampCreation(CreatedAtUtc, createdByUserId);
    }

    public void SetModifiedAudit(DateTimeOffset modifiedAtUtc, UserId? modifiedByUserId)
    {
        LastModifiedAtUtc = modifiedAtUtc.ToUniversalTime();
        LastModifiedByUserId = modifiedByUserId;
    }

    private static Result ValidateContractData(
        TrainingContractId id,
        OrganizationId organizationId,
        BranchId branchId,
        PersonId studentId,
        CommercialOfferId sourceOfferId,
        int sourceOfferVersion,
        string contractNumber,
        DateOnly startDate,
        DateOnly? endDate,
        decimal totalAmount,
        string currency,
        TrainingContractTermsSnapshot termsSnapshot,
        IReadOnlyCollection<TrainingContractParty> parties)
    {
        if (id.IsEmpty)
            return Result.Failure(TrainingContractErrors.InvalidIdentifier);

        if (organizationId.IsEmpty || branchId.IsEmpty || studentId.IsEmpty)
            return Result.Failure(TrainingContractErrors.InvalidOwner);

        if (sourceOfferId.IsEmpty || sourceOfferVersion < 1)
            return Result.Failure(TrainingContractErrors.InvalidSourceOffer);

        string normalizedNumber = contractNumber?.Trim().ToUpperInvariant() ?? string.Empty;
        if (normalizedNumber.Length is < 3 or > 100)
            return Result.Failure(TrainingContractErrors.InvalidContractNumber);

        if (endDate is not null && endDate < startDate)
            return Result.Failure(TrainingContractErrors.InvalidEffectivePeriod);

        if (totalAmount < 0 || totalAmount > 100_000_000m)
            return Result.Failure(TrainingContractErrors.InvalidAmount);

        string normalizedCurrency = currency?.Trim().ToUpperInvariant() ?? string.Empty;
        if (normalizedCurrency.Length != 3 || normalizedCurrency.Any(c => c is < 'A' or > 'Z'))
            return Result.Failure(TrainingContractErrors.InvalidCurrency);

        if (termsSnapshot is null)
            return Result.Failure(TrainingContractErrors.InvalidTermsSnapshot);

        if (parties is null || parties.Count == 0)
            return Result.Failure(TrainingContractErrors.ProviderPartyRequired);

        if (!parties.Any(p => p.Kind == TrainingContractPartyKind.TrainingProvider &&
                              p.OrganizationId == organizationId))
            return Result.Failure(TrainingContractErrors.ProviderPartyRequired);

        if (!parties.Any(p => p.Kind == TrainingContractPartyKind.Student &&
                              p.PersonId == studentId))
            return Result.Failure(TrainingContractErrors.StudentPartyRequired);

        if (HasDuplicateParties(parties))
            return Result.Failure(TrainingContractErrors.DuplicateParty);

        return Result.Success();
    }

    private static bool HasDuplicateParties(IEnumerable<TrainingContractParty> parties)
    {
        return parties
            .GroupBy(p => new
            {
                p.Kind,
                Person = p.PersonId?.Value,
                Organization = p.OrganizationId?.Value
            })
            .Any(group => group.Count() > 1);
    }
}
