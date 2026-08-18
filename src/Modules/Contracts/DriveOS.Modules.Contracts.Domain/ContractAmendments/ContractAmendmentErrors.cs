using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Contracts.Domain.ContractAmendments;

public static class ContractAmendmentErrors
{
    public static readonly Error InvalidIdentifier = Error.Validation("Contracts.Amendment.Id.Invalid", "errors.contracts.amendment.id.invalid");
    public static readonly Error InvalidOwner = Error.Validation("Contracts.Amendment.Owner.Invalid", "errors.contracts.amendment.owner.invalid");
    public static readonly Error InvalidNumber = Error.Validation("Contracts.Amendment.Number.Invalid", "errors.contracts.amendment.number.invalid");
    public static readonly Error InvalidBaseVersion = Error.Validation("Contracts.Amendment.BaseVersion.Invalid", "errors.contracts.amendment.baseVersion.invalid");
    public static readonly Error InvalidReason = Error.Validation("Contracts.Amendment.Reason.Invalid", "errors.contracts.amendment.reason.invalid");
    public static readonly Error InvalidEffectiveDate = Error.Validation("Contracts.Amendment.EffectiveDate.Invalid", "errors.contracts.amendment.effectiveDate.invalid");
    public static readonly Error InvalidSnapshot = Error.Validation("Contracts.Amendment.Snapshot.Invalid", "errors.contracts.amendment.snapshot.invalid");
    public static readonly Error InvalidSignedDocument = Error.Validation("Contracts.Amendment.SignedDocument.Invalid", "errors.contracts.amendment.signedDocument.invalid");
    public static readonly Error SignNotAllowed = Error.Conflict("Contracts.Amendment.Sign.NotAllowed", "errors.contracts.amendment.sign.notAllowed");
    public static readonly Error ApplyNotAllowed = Error.Conflict("Contracts.Amendment.Apply.NotAllowed", "errors.contracts.amendment.apply.notAllowed");
    public static readonly Error CancelNotAllowed = Error.Conflict("Contracts.Amendment.Cancel.NotAllowed", "errors.contracts.amendment.cancel.notAllowed");
    public static readonly Error BaseVersionChanged = Error.Conflict("Contracts.Amendment.BaseVersion.Changed", "errors.contracts.amendment.baseVersion.changed");
    public static readonly Error NotFound = Error.NotFound("Contracts.Amendment.NotFound", "errors.contracts.amendment.notFound");
}
