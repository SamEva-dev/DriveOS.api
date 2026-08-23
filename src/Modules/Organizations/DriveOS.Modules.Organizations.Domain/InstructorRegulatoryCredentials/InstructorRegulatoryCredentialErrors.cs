using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain.InstructorRegulatoryCredentials;

public static class InstructorRegulatoryCredentialErrors
{
    public static readonly Error InvalidOwner = Error.Validation("InstructorRegulatoryCredentials.Owner.Invalid", "errors.instructorRegulatoryCredentials.owner.invalid");
    public static readonly Error InvalidCredential = Error.Validation("InstructorRegulatoryCredentials.Credential.Invalid", "errors.instructorRegulatoryCredentials.credential.invalid");
    public static readonly Error InvalidIssuingAuthority = Error.Validation("InstructorRegulatoryCredentials.IssuingAuthority.Invalid", "errors.instructorRegulatoryCredentials.issuingAuthority.invalid");
    public static readonly Error InvalidValidityPeriod = Error.Validation("InstructorRegulatoryCredentials.Validity.Invalid", "errors.instructorRegulatoryCredentials.validity.invalid");
    public static readonly Error VerificationMethodRequired = Error.Validation("InstructorRegulatoryCredentials.VerificationMethod.Required", "errors.instructorRegulatoryCredentials.verificationMethod.required");
    public static readonly Error DecisionReasonRequired = Error.Validation("InstructorRegulatoryCredentials.DecisionReason.Required", "errors.instructorRegulatoryCredentials.decisionReason.required");
    public static readonly Error NotCurrent = Error.Conflict("InstructorRegulatoryCredentials.NotCurrent", "errors.instructorRegulatoryCredentials.notCurrent");
    public static readonly Error InvalidReplacement = Error.Validation("InstructorRegulatoryCredentials.Replacement.Invalid", "errors.instructorRegulatoryCredentials.replacement.invalid");
    public static readonly Error NotFound = Error.NotFound("InstructorRegulatoryCredentials.NotFound", "errors.instructorRegulatoryCredentials.notFound");
    public static readonly Error InstructorNotAssigned = Error.NotFound("InstructorRegulatoryCredentials.InstructorNotAssigned", "errors.instructorRegulatoryCredentials.instructorNotAssigned");
}
