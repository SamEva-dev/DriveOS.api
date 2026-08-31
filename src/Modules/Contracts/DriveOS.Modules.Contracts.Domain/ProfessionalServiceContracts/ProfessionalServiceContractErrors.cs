using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.Contracts.Domain.ProfessionalServiceContracts;
public static class ProfessionalServiceContractErrors
{
    public static readonly Error NotFound=Error.NotFound("Contracts.ProfessionalServiceContract.NotFound","errors.contracts.professionalServiceContract.notFound");
    public static readonly Error Invalid=Error.Validation("Contracts.ProfessionalServiceContract.Invalid","errors.contracts.professionalServiceContract.invalid");
    public static readonly Error Duplicate=Error.Conflict("Contracts.ProfessionalServiceContract.Duplicate","errors.contracts.professionalServiceContract.duplicate");
    public static readonly Error InvalidTransition=Error.Conflict("Contracts.ProfessionalServiceContract.InvalidTransition","errors.contracts.professionalServiceContract.invalidTransition");
    public static readonly Error InvalidDocument=Error.Validation("Contracts.ProfessionalServiceContract.InvalidDocument","errors.contracts.professionalServiceContract.invalidDocument");
    public static readonly Error DocumentHashMismatch=Error.Conflict("Contracts.ProfessionalServiceContract.DocumentHashMismatch","errors.contracts.professionalServiceContract.documentHashMismatch");
    public static readonly Error SignatoryNotFound=Error.NotFound("Contracts.ProfessionalServiceContract.SignatoryNotFound","errors.contracts.professionalServiceContract.signatoryNotFound");
    public static readonly Error AlreadySigned=Error.Conflict("Contracts.ProfessionalServiceContract.AlreadySigned","errors.contracts.professionalServiceContract.alreadySigned");
    public static readonly Error SigningOrderViolation=Error.Conflict("Contracts.ProfessionalServiceContract.SigningOrderViolation","errors.contracts.professionalServiceContract.signingOrderViolation");
    public static readonly Error InvalidSignatureEvidence=Error.Validation("Contracts.ProfessionalServiceContract.InvalidSignatureEvidence","errors.contracts.professionalServiceContract.invalidSignatureEvidence");
    public static readonly Error ReasonRequired=Error.Validation("Contracts.ProfessionalServiceContract.ReasonRequired","errors.contracts.professionalServiceContract.reasonRequired");
    public static readonly Error RevisionRequiresSignature=Error.Conflict("Contracts.ProfessionalServiceContract.RevisionRequiresSignature","errors.contracts.professionalServiceContract.revisionRequiresSignature");
}
