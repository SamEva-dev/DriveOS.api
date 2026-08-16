using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Domain.Students;

public static class StudentErrors
{
    public static readonly Error InvalidId = Error.Validation(
        "Students.Student.Id.Invalid",
        "errors.students.student.id.invalid"
    );
    public static readonly Error InvalidOrganization = Error.Validation(
        "Students.Student.Organization.Invalid",
        "errors.students.student.organization.invalid"
    );
    public static readonly Error FirstNameRequired = Error.Validation(
        "Students.Student.FirstName.Required",
        "errors.students.student.firstName.required"
    );
    public static readonly Error LastNameRequired = Error.Validation(
        "Students.Student.LastName.Required",
        "errors.students.student.lastName.required"
    );
    public static readonly Error IdentityTooLong = Error.Validation(
        "Students.Student.Identity.TooLong",
        "errors.students.student.identity.tooLong"
    );
    public static readonly Error LegalNameRequired = Error.Validation(
        "Students.Identity.LegalName.Required",
        "errors.students.identity.legalName.required"
    );
    public static readonly Error VerifiedIdentityJustificationRequired = Error.Validation(
        "Students.Identity.Verified.ChangeJustification.Required",
        "errors.students.identity.verified.changeJustification.required"
    );
    public static readonly Error InvalidVerificationStatus = Error.Validation(
        "Students.Identity.VerificationStatus.Invalid",
        "errors.students.identity.verificationStatus.invalid"
    );
    public static readonly Error VerificationJustificationRequired = Error.Validation(
        "Students.Identity.VerificationJustification.Required",
        "errors.students.identity.verificationJustification.required"
    );
}
