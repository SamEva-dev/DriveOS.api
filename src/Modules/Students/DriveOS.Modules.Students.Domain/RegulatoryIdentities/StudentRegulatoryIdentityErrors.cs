using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Domain.RegulatoryIdentities;

public static class StudentRegulatoryIdentityErrors
{
    public static readonly Error InvalidOwner = Error.Validation(
        "Students.RegulatoryIdentity.Owner.Invalid",
        "errors.students.regulatoryIdentity.owner.invalid");

    public static readonly Error InvalidIdentifier = Error.Validation(
        "Students.RegulatoryIdentity.Identifier.Invalid",
        "errors.students.regulatoryIdentity.identifier.invalid");

    public static readonly Error StudentNotFound = Error.NotFound(
        "Students.RegulatoryIdentity.Student.NotFound",
        "errors.students.regulatoryIdentity.student.notFound");

    public static readonly Error NotFound = Error.NotFound(
        "Students.RegulatoryIdentity.NotFound",
        "errors.students.regulatoryIdentity.notFound");

    public static readonly Error NotCurrent = Error.Conflict(
        "Students.RegulatoryIdentity.NotCurrent",
        "errors.students.regulatoryIdentity.notCurrent");

    public static readonly Error VerificationMethodRequired = Error.Validation(
        "Students.RegulatoryIdentity.VerificationMethod.Required",
        "errors.students.regulatoryIdentity.verificationMethod.required");

    public static readonly Error DecisionReasonRequired = Error.Validation(
        "Students.RegulatoryIdentity.DecisionReason.Required",
        "errors.students.regulatoryIdentity.decisionReason.required");

    public static readonly Error InvalidReplacement = Error.Validation(
        "Students.RegulatoryIdentity.Replacement.Invalid",
        "errors.students.regulatoryIdentity.replacement.invalid");
}
