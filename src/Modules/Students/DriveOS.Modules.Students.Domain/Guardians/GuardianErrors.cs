using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Domain.Guardians;

public static class GuardianErrors
{
    public static readonly Error InvalidOwner = Error.Validation(
        "Students.Guardian.Owner.Invalid",
        "errors.students.guardian.owner.invalid"
    );
    public static readonly Error RequiredData = Error.Validation(
        "Students.Guardian.Data.Required",
        "errors.students.guardian.data.required"
    );
    public static readonly Error InvalidPeriod = Error.Validation(
        "Students.Guardian.Period.Invalid",
        "errors.students.guardian.period.invalid"
    );
    public static readonly Error NotFound = Error.NotFound(
        "Students.Guardian.NotFound",
        "errors.students.guardian.notFound"
    );
    public static readonly Error Revoked = Error.Conflict(
        "Students.Guardian.Revoked",
        "errors.students.guardian.revoked"
    );
    public static readonly Error NotActive = Error.Conflict(
        "Students.Guardian.NotActive",
        "errors.students.guardian.notActive"
    );
    public static readonly Error ReasonRequired = Error.Validation(
        "Students.Guardian.RevocationReason.Required",
        "errors.students.guardian.revocationReason.required"
    );
    public static readonly Error InvitationContactRequired = Error.Validation(
        "Students.Guardian.InvitationContact.Required",
        "errors.students.guardian.invitationContact.required"
    );
}
