using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Contracts.Domain.TrainingContracts;

public static class TrainingContractErrors
{
    public static readonly Error InvalidIdentifier = Error.Validation(
        "Contracts.TrainingContract.Id.Invalid",
        "errors.contracts.trainingContract.id.invalid");

    public static readonly Error InvalidOwner = Error.Validation(
        "Contracts.TrainingContract.Owner.Invalid",
        "errors.contracts.trainingContract.owner.invalid");

    public static readonly Error InvalidSourceOffer = Error.Validation(
        "Contracts.TrainingContract.SourceOffer.Invalid",
        "errors.contracts.trainingContract.sourceOffer.invalid");

    public static readonly Error InvalidContractNumber = Error.Validation(
        "Contracts.TrainingContract.Number.Invalid",
        "errors.contracts.trainingContract.number.invalid");

    public static readonly Error InvalidEffectivePeriod = Error.Validation(
        "Contracts.TrainingContract.EffectivePeriod.Invalid",
        "errors.contracts.trainingContract.effectivePeriod.invalid");

    public static readonly Error InvalidAmount = Error.Validation(
        "Contracts.TrainingContract.Amount.Invalid",
        "errors.contracts.trainingContract.amount.invalid");

    public static readonly Error InvalidCurrency = Error.Validation(
        "Contracts.TrainingContract.Currency.Invalid",
        "errors.contracts.trainingContract.currency.invalid");

    public static readonly Error InvalidTermsSnapshot = Error.Validation(
        "Contracts.TrainingContract.TermsSnapshot.Invalid",
        "errors.contracts.trainingContract.termsSnapshot.invalid");

    public static readonly Error InvalidParty = Error.Validation(
        "Contracts.TrainingContract.Party.Invalid",
        "errors.contracts.trainingContract.party.invalid");

    public static readonly Error ProviderPartyRequired = Error.Validation(
        "Contracts.TrainingContract.ProviderParty.Required",
        "errors.contracts.trainingContract.providerParty.required");

    public static readonly Error StudentPartyRequired = Error.Validation(
        "Contracts.TrainingContract.StudentParty.Required",
        "errors.contracts.trainingContract.studentParty.required");

    public static readonly Error DuplicateParty = Error.Conflict(
        "Contracts.TrainingContract.Party.Duplicate",
        "errors.contracts.trainingContract.party.duplicate");


    public static readonly Error InvalidVersion = Error.Validation(
        "Contracts.TrainingContract.Version.Invalid",
        "errors.contracts.trainingContract.version.invalid");

    public static readonly Error InvalidRevisionReason = Error.Validation(
        "Contracts.TrainingContract.RevisionReason.Invalid",
        "errors.contracts.trainingContract.revisionReason.invalid");

    public static readonly Error RevisionLocked = Error.Conflict(
        "Contracts.TrainingContract.Revision.Locked",
        "errors.contracts.trainingContract.revision.locked");

    public static readonly Error GenerationNotAllowed = Error.Conflict(
        "Contracts.TrainingContract.Generation.NotAllowed",
        "errors.contracts.trainingContract.generation.notAllowed");

    public static readonly Error InvalidGeneratedDocument = Error.Validation(
        "Contracts.TrainingContract.GeneratedDocument.Invalid",
        "errors.contracts.trainingContract.generatedDocument.invalid");

    public static readonly Error NotFound = Error.NotFound(
        "Contracts.TrainingContract.NotFound",
        "errors.contracts.trainingContract.notFound");

    public static readonly Error InvalidSignatory = Error.Validation("Contracts.TrainingContract.Signatory.Invalid", "errors.contracts.trainingContract.signatory.invalid");
    public static readonly Error SignatoryOrganizationRequired = Error.Validation("Contracts.TrainingContract.Signatory.OrganizationRequired", "errors.contracts.trainingContract.signatory.organizationRequired");
    public static readonly Error DuplicateSignatory = Error.Conflict("Contracts.TrainingContract.Signatory.Duplicate", "errors.contracts.trainingContract.signatory.duplicate");
    public static readonly Error SignatoryNotFound = Error.NotFound("Contracts.TrainingContract.Signatory.NotFound", "errors.contracts.trainingContract.signatory.notFound");
    public static readonly Error SignatoryManagementLocked = Error.Conflict("Contracts.TrainingContract.Signatory.ManagementLocked", "errors.contracts.trainingContract.signatory.managementLocked");
    public static readonly Error SignatoryLocked = Error.Conflict("Contracts.TrainingContract.Signatory.Locked", "errors.contracts.trainingContract.signatory.locked");
    public static readonly Error InvalidAuthorityDecision = Error.Validation("Contracts.TrainingContract.Signatory.AuthorityDecision.Invalid", "errors.contracts.trainingContract.signatory.authorityDecision.invalid");
    public static readonly Error SendForSignatureNotAllowed = Error.Conflict("Contracts.TrainingContract.Signature.SendNotAllowed", "errors.contracts.trainingContract.signature.sendNotAllowed");
    public static readonly Error InvalidSignatureProcess = Error.Validation("Contracts.TrainingContract.Signature.ProcessInvalid", "errors.contracts.trainingContract.signature.processInvalid");
    public static readonly Error GeneratedDocumentOutdated = Error.Conflict("Contracts.TrainingContract.Signature.DocumentOutdated", "errors.contracts.trainingContract.signature.documentOutdated");
    public static readonly Error RequiredSignatoryMissing = Error.Validation("Contracts.TrainingContract.Signature.RequiredSignatoryMissing", "errors.contracts.trainingContract.signature.requiredSignatoryMissing");
    public static readonly Error SignatoryNotReady = Error.Conflict("Contracts.TrainingContract.Signature.SignatoryNotReady", "errors.contracts.trainingContract.signature.signatoryNotReady");
    public static readonly Error SignatureRecordingNotAllowed = Error.Conflict("Contracts.TrainingContract.Signature.RecordNotAllowed", "errors.contracts.trainingContract.signature.recordNotAllowed");
    public static readonly Error InvalidSignatureEvidence = Error.Validation("Contracts.TrainingContract.Signature.EvidenceInvalid", "errors.contracts.trainingContract.signature.evidenceInvalid");
    public static readonly Error ActivationNotAllowed = Error.Conflict("Contracts.TrainingContract.Activation.NotAllowed", "errors.contracts.trainingContract.activation.notAllowed");
    public static readonly Error InvalidActivation = Error.Validation("Contracts.TrainingContract.Activation.Invalid", "errors.contracts.trainingContract.activation.invalid");
    public static readonly Error ActivationBeforeStartDate = Error.Conflict("Contracts.TrainingContract.Activation.BeforeStartDate", "errors.contracts.trainingContract.activation.beforeStartDate");
    public static readonly Error ActivationAfterEndDate = Error.Conflict("Contracts.TrainingContract.Activation.AfterEndDate", "errors.contracts.trainingContract.activation.afterEndDate");
    public static readonly Error AmendmentNotAllowed = Error.Conflict("Contracts.TrainingContract.Amendment.NotAllowed", "errors.contracts.trainingContract.amendment.notAllowed");
    public static readonly Error InvalidAmendment = Error.Validation("Contracts.TrainingContract.Amendment.Invalid", "errors.contracts.trainingContract.amendment.invalid");
    public static readonly Error AmendmentNotSigned = Error.Conflict("Contracts.TrainingContract.Amendment.NotSigned", "errors.contracts.trainingContract.amendment.notSigned");
    public static readonly Error AmendmentNotEffectiveYet = Error.Conflict("Contracts.TrainingContract.Amendment.NotEffectiveYet", "errors.contracts.trainingContract.amendment.notEffectiveYet");

    public static readonly Error SuspensionNotAllowed = Error.Conflict("Contracts.TrainingContract.Suspension.NotAllowed", "errors.contracts.trainingContract.suspension.notAllowed");
    public static readonly Error InvalidSuspension = Error.Validation("Contracts.TrainingContract.Suspension.Invalid", "errors.contracts.trainingContract.suspension.invalid");
    public static readonly Error SuspensionEffectiveDateMustBeToday = Error.Validation("Contracts.TrainingContract.Suspension.EffectiveDateMustBeToday", "errors.contracts.trainingContract.suspension.effectiveDateMustBeToday");
    public static readonly Error SuspensionAfterContractEnd = Error.Conflict("Contracts.TrainingContract.Suspension.AfterContractEnd", "errors.contracts.trainingContract.suspension.afterContractEnd");
    public static readonly Error InvalidSuspensionResumeDate = Error.Validation("Contracts.TrainingContract.Suspension.ResumeDateInvalid", "errors.contracts.trainingContract.suspension.resumeDateInvalid");
    public static readonly Error SuspensionResumeAfterContractEnd = Error.Validation("Contracts.TrainingContract.Suspension.ResumeAfterContractEnd", "errors.contracts.trainingContract.suspension.resumeAfterContractEnd");

    public static readonly Error TerminationNotAllowed = Error.Conflict("Contracts.TrainingContract.Termination.NotAllowed", "errors.contracts.trainingContract.termination.notAllowed");
    public static readonly Error InvalidTermination = Error.Validation("Contracts.TrainingContract.Termination.Invalid", "errors.contracts.trainingContract.termination.invalid");
    public static readonly Error TerminationEffectiveDateMustBeToday = Error.Validation("Contracts.TrainingContract.Termination.EffectiveDateMustBeToday", "errors.contracts.trainingContract.termination.effectiveDateMustBeToday");
    public static readonly Error TerminationBeforeContractStart = Error.Conflict("Contracts.TrainingContract.Termination.BeforeContractStart", "errors.contracts.trainingContract.termination.beforeContractStart");

    public static readonly Error CompletionNotAllowed = Error.Conflict("Contracts.TrainingContract.Completion.NotAllowed", "errors.contracts.trainingContract.completion.notAllowed");
    public static readonly Error InvalidCompletion = Error.Validation("Contracts.TrainingContract.Completion.Invalid", "errors.contracts.trainingContract.completion.invalid");
    public static readonly Error CompletionEffectiveDateMustBeToday = Error.Validation("Contracts.TrainingContract.Completion.EffectiveDateMustBeToday", "errors.contracts.trainingContract.completion.effectiveDateMustBeToday");
    public static readonly Error CompletionBeforeContractStart = Error.Conflict("Contracts.TrainingContract.Completion.BeforeContractStart", "errors.contracts.trainingContract.completion.beforeContractStart");
    public static readonly Error CompletionAfterContractEnd = Error.Conflict("Contracts.TrainingContract.Completion.AfterContractEnd", "errors.contracts.trainingContract.completion.afterContractEnd");

    public static readonly Error ExpirationNotAllowed = Error.Conflict("Contracts.TrainingContract.Expiration.NotAllowed", "errors.contracts.trainingContract.expiration.notAllowed");
    public static readonly Error InvalidExpiration = Error.Validation("Contracts.TrainingContract.Expiration.Invalid", "errors.contracts.trainingContract.expiration.invalid");
    public static readonly Error ExpirationRequiresEndDate = Error.Validation("Contracts.TrainingContract.Expiration.EndDateRequired", "errors.contracts.trainingContract.expiration.endDateRequired");
    public static readonly Error ContractNotExpiredYet = Error.Conflict("Contracts.TrainingContract.Expiration.NotExpiredYet", "errors.contracts.trainingContract.expiration.notExpiredYet");

}
