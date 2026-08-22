using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Domain.Certifications;

public static class ExamAttestationErrors
{
    public static readonly Error InvalidIdentifier = Error.Validation("exam-attestation.invalid-identifier", "exams.attestations.errors.invalidIdentifier");
    public static readonly Error InvalidDocument = Error.Validation("exam-attestation.invalid-document", "exams.attestations.errors.invalidDocument");
    public static readonly Error InvalidType = Error.Validation("exam-attestation.invalid-type", "exams.attestations.errors.invalidType");
    public static readonly Error InvalidSignature = Error.Validation("exam-attestation.invalid-signature", "exams.attestations.errors.invalidSignature");
    public static readonly Error InvalidDelivery = Error.Validation("exam-attestation.invalid-delivery", "exams.attestations.errors.invalidDelivery");
    public static readonly Error SuccessRequired = Error.Conflict("exam-attestation.success-required", "exams.attestations.errors.successRequired");
    public static readonly Error FinalizedResultRequired = Error.Conflict("exam-attestation.finalized-result-required", "exams.attestations.errors.finalizedResultRequired");
    public static readonly Error AlreadyRevoked = Error.Conflict("exam-attestation.already-revoked", "exams.attestations.errors.alreadyRevoked");
    public static readonly Error NotActive = Error.Conflict("exam-attestation.not-active", "exams.attestations.errors.notActive");
    public static readonly Error SignatureRequired = Error.Conflict("exam-attestation.signature-required", "exams.attestations.errors.signatureRequired");
    public static readonly Error RevocationReasonRequired = Error.Validation("exam-attestation.revocation-reason-required", "exams.attestations.errors.revocationReasonRequired");
    public static readonly Error NotFound = Error.NotFound("exam-attestation.not-found", "exams.attestations.errors.notFound");
    public static readonly Error OperationConflict = Error.Conflict("exam-attestation.operation-conflict", "exams.attestations.errors.operationConflict");
    public static readonly Error VerificationTokenRequired = Error.Validation("exam-attestation.verification-token-required", "exams.attestations.errors.verificationTokenRequired");
}
